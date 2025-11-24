using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;

namespace Dream.Editor
{
    /// <summary>
    /// 动画Clip提取工具
    /// 用于从FBX模型中提取动画clip并复制到指定路径
    /// 支持批量处理文件夹下的所有FBX文件
    /// </summary>
    public class AnimationClipExtractor : EditorWindow
    {
        private Object sourceFolder;
        private Object targetFolder;
        private Vector2 scrollPosition;
        private List<string> extractedClips = new List<string>();
        private bool showResults = false;
        private bool overwriteExisting = false;
        private bool includeSubfolders = false;

        [MenuItem("Dream Tools/动画Clip提取工具")]
        public static void ShowWindow()
        {
            GetWindow<AnimationClipExtractor>("动画Clip提取工具");
        }

        private void OnGUI()
        {
            GUILayout.Label("动画Clip提取工具", EditorStyles.boldLabel);
            EditorGUILayout.HelpBox("选择包含FBX文件的文件夹，工具会自动提取所有FBX文件中的动画clip到输出文件夹。", MessageType.Info);
            EditorGUILayout.Space();

            // 源文件夹选择
            EditorGUILayout.LabelField("源文件夹（包含FBX文件）:", EditorStyles.boldLabel);
            sourceFolder = EditorGUILayout.ObjectField(sourceFolder, typeof(DefaultAsset), false);
            if (sourceFolder != null)
            {
                string assetPath = AssetDatabase.GetAssetPath(sourceFolder);
                if (!AssetDatabase.IsValidFolder(assetPath))
                {
                    EditorGUILayout.HelpBox("请选择文件夹！", MessageType.Warning);
                }
                else
                {
                    // 统计FBX文件数量
                    int fbxCount = GetFBXFilesCount(assetPath);
                    EditorGUILayout.HelpBox($"源路径: {assetPath}\n找到 {fbxCount} 个FBX文件", MessageType.Info);
                }
            }

            EditorGUILayout.Space();

            // 目标文件夹选择
            EditorGUILayout.LabelField("输出文件夹:", EditorStyles.boldLabel);
            targetFolder = EditorGUILayout.ObjectField(targetFolder, typeof(DefaultAsset), false);
            if (targetFolder != null)
            {
                string assetPath = AssetDatabase.GetAssetPath(targetFolder);
                if (!AssetDatabase.IsValidFolder(assetPath))
                {
                    EditorGUILayout.HelpBox("请选择文件夹！", MessageType.Warning);
                }
                else
                {
                    EditorGUILayout.HelpBox($"输出路径: {assetPath}", MessageType.Info);
                }
            }

            EditorGUILayout.Space();

            // 选项
            includeSubfolders = EditorGUILayout.Toggle("包含子文件夹", includeSubfolders);
            overwriteExisting = EditorGUILayout.Toggle("覆盖已存在的文件", overwriteExisting);
            EditorGUILayout.Space();

            // 提取按钮
            bool canExtract = sourceFolder != null && targetFolder != null;
            if (canExtract)
            {
                string sourcePath = AssetDatabase.GetAssetPath(sourceFolder);
                string targetPath = AssetDatabase.GetAssetPath(targetFolder);
                canExtract = AssetDatabase.IsValidFolder(sourcePath) && AssetDatabase.IsValidFolder(targetPath);
            }

            GUI.enabled = canExtract;
            if (GUILayout.Button("提取动画Clip", GUILayout.Height(30)))
            {
                ExtractAnimationClips();
            }
            GUI.enabled = true;

            EditorGUILayout.Space();

            // 结果显示
            if (showResults)
            {
                EditorGUILayout.LabelField("提取结果:", EditorStyles.boldLabel);
                scrollPosition = EditorGUILayout.BeginScrollView(scrollPosition, GUILayout.Height(200));
                foreach (var clip in extractedClips)
                {
                    EditorGUILayout.LabelField(clip, EditorStyles.wordWrappedLabel);
                }
                EditorGUILayout.EndScrollView();
            }
        }

        private int GetFBXFilesCount(string folderPath)
        {
            int count = 0;
            string[] guids = AssetDatabase.FindAssets("t:GameObject", new[] { folderPath });
            foreach (string guid in guids)
            {
                string path = AssetDatabase.GUIDToAssetPath(guid);
                if (path.EndsWith(".fbx", System.StringComparison.OrdinalIgnoreCase))
                {
                    // 只统计当前文件夹下的文件（不包括子文件夹）
                    string relativePath = path.Substring(folderPath.Length + 1);
                    if (!relativePath.Contains("/"))
                    {
                        count++;
                    }
                }
            }
            return count;
        }

        private List<string> GetAllFBXFiles(string folderPath, bool includeSubfolders)
        {
            List<string> fbxFiles = new List<string>();
            
            // 使用AssetDatabase.FindAssets查找所有FBX文件
            string[] guids = AssetDatabase.FindAssets("t:GameObject", new[] { folderPath });
            foreach (string guid in guids)
            {
                string path = AssetDatabase.GUIDToAssetPath(guid);
                if (path.EndsWith(".fbx", System.StringComparison.OrdinalIgnoreCase))
                {
                    // 检查是否在子文件夹中（如果不需要包含子文件夹）
                    if (!includeSubfolders)
                    {
                        // 确保路径在指定文件夹下且不在子文件夹中
                        if (!path.StartsWith(folderPath + "/"))
                        {
                            continue;
                        }
                        string relativePath = path.Substring(folderPath.Length + 1);
                        if (relativePath.Contains("/"))
                        {
                            continue;
                        }
                    }
                    else
                    {
                        // 确保路径在指定文件夹下（包括子文件夹）
                        if (!path.StartsWith(folderPath + "/"))
                        {
                            continue;
                        }
                    }
                    fbxFiles.Add(path);
                }
            }
            return fbxFiles;
        }

        private void ExtractAnimationClips()
        {
            extractedClips.Clear();
            showResults = false;

            // 获取路径
            if (sourceFolder == null || targetFolder == null)
            {
                EditorUtility.DisplayDialog("错误", "请选择源文件夹和输出文件夹！", "确定");
                return;
            }

            string sourcePath = AssetDatabase.GetAssetPath(sourceFolder);
            string targetPath = AssetDatabase.GetAssetPath(targetFolder);

            // 验证源文件夹
            if (!AssetDatabase.IsValidFolder(sourcePath))
            {
                EditorUtility.DisplayDialog("错误", "请选择有效的源文件夹！", "确定");
                return;
            }

            // 验证目标文件夹
            if (!AssetDatabase.IsValidFolder(targetPath))
            {
                EditorUtility.DisplayDialog("错误", "请选择有效的输出文件夹！", "确定");
                return;
            }

            // 确保目标文件夹存在
            if (!Directory.Exists(targetPath))
            {
                Directory.CreateDirectory(targetPath);
                AssetDatabase.Refresh();
            }

            // 获取所有FBX文件
            List<string> fbxFiles = GetAllFBXFiles(sourcePath, includeSubfolders);
            
            if (fbxFiles.Count == 0)
            {
                EditorUtility.DisplayDialog("提示", "在选择的文件夹中没有找到FBX文件！", "确定");
                return;
            }

            int totalSuccessCount = 0;
            int totalFailCount = 0;
            int processedFiles = 0;

            extractedClips.Add($"开始处理 {fbxFiles.Count} 个FBX文件...");
            extractedClips.Add("");

            // 遍历每个FBX文件
            foreach (string fbxPath in fbxFiles)
            {
                processedFiles++;
                string fbxName = Path.GetFileNameWithoutExtension(fbxPath);
                extractedClips.Add($"━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━");
                extractedClips.Add($"处理文件 [{processedFiles}/{fbxFiles.Count}]: {fbxName}");

                try
                {
                    // 获取ModelImporter
                    ModelImporter importer = AssetImporter.GetAtPath(fbxPath) as ModelImporter;
                    if (importer == null)
                    {
                        extractedClips.Add($"  ✗ 无法获取导入器，跳过");
                        totalFailCount++;
                        continue;
                    }

                    // 加载FBX文件的所有子资源（包括动画clip）
                    Object[] allAssets = AssetDatabase.LoadAllAssetsAtPath(fbxPath);
                    List<AnimationClip> animationClips = new List<AnimationClip>();

                    foreach (Object asset in allAssets)
                    {
                        if (asset is AnimationClip)
                        {
                            animationClips.Add(asset as AnimationClip);
                        }
                    }

                    if (animationClips.Count == 0)
                    {
                        extractedClips.Add($"  ⊘ 未找到动画clip");
                        continue;
                    }

                    int fileSuccessCount = 0;
                    int fileFailCount = 0;

                    // 提取每个动画clip
                    foreach (var clip in animationClips)
                    {
                        try
                        {
                            // 跳过内部clip（Unity自动生成的预览clip）
                            if (clip.name.StartsWith("__preview__"))
                            {
                                continue;
                            }

                            // 生成输出路径（保持原有动画clip名称）
                            string fileName = $"{clip.name}.anim";
                            string outputPath = Path.Combine(targetPath, fileName).Replace("\\", "/");

                            // 检查文件是否已存在
                            if (AssetDatabase.LoadAssetAtPath<AnimationClip>(outputPath) != null)
                            {
                                if (!overwriteExisting)
                                {
                                    extractedClips.Add($"  ⊘ {clip.name} - 已跳过（文件已存在）");
                                    continue;
                                }
                                // 如果选择覆盖，删除旧文件
                                AssetDatabase.DeleteAsset(outputPath);
                            }

                            // 创建动画clip的副本
                            AnimationClip newClip = Object.Instantiate(clip);
                            newClip.name = clip.name;

                            // 保存动画clip
                            AssetDatabase.CreateAsset(newClip, outputPath);
                            
                            extractedClips.Add($"  ✓ {clip.name} -> {Path.GetFileName(outputPath)}");
                            fileSuccessCount++;
                            totalSuccessCount++;
                        }
                        catch (System.Exception e)
                        {
                            extractedClips.Add($"  ✗ {clip.name} - 错误: {e.Message}");
                            fileFailCount++;
                            totalFailCount++;
                            Debug.LogError($"提取动画clip失败: {fbxName}/{clip.name}, 错误: {e}");
                        }
                    }

                    if (fileSuccessCount > 0 || fileFailCount > 0)
                    {
                        extractedClips.Add($"  结果: 成功 {fileSuccessCount} 个, 失败 {fileFailCount} 个");
                    }
                }
                catch (System.Exception e)
                {
                    extractedClips.Add($"  ✗ 处理文件时出错: {e.Message}");
                    totalFailCount++;
                    Debug.LogError($"处理FBX文件失败: {fbxPath}, 错误: {e}");
                }
            }

            // 保存并刷新资源数据库
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();

            showResults = true;

            // 显示结果对话框
            string message = $"批量提取完成！\n处理文件: {fbxFiles.Count} 个\n成功: {totalSuccessCount} 个动画clip\n失败: {totalFailCount} 个";
            EditorUtility.DisplayDialog("提取完成", message, "确定");
        }
    }
}

