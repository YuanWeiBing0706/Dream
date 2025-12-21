using UnityEditor;
using UnityEngine;

namespace Editor
{
    public class FileRenamer : EditorWindow
    {
        private string prefix = "Wep_"; // 默认前缀
        private string suffix = "";     // 默认后缀
        private string replaceOld = ""; // 要替换的旧字符
        private string replaceNew = ""; // 替换成的新字符

        [MenuItem("Tools/批量重命名工具")]
        public static void ShowWindow()
        {
            GetWindow<FileRenamer>("批量重命名");
        }

        private void OnGUI()
        {
            GUILayout.Label("批量重命名 (选中Project窗口里的文件)", EditorStyles.boldLabel);
            EditorGUILayout.Space();

            // --- 模式 1: 添加前缀/后缀 ---
            GUILayout.Label("模式 A: 添加前缀/后缀");
            prefix = EditorGUILayout.TextField("前缀 (Prefix):", prefix);
            suffix = EditorGUILayout.TextField("后缀 (Suffix):", suffix);

            if (GUILayout.Button("应用前缀后缀"))
            {
                RenameSelectedAssets(assetPath =>
                {
                    string dir = System.IO.Path.GetDirectoryName(assetPath);
                    string fileName = System.IO.Path.GetFileNameWithoutExtension(assetPath);
                    string ext = System.IO.Path.GetExtension(assetPath);
                
                    // 只有当名字不包含这个前缀时才加，防止重复加 Wep_Wep_
                    string newName = fileName;
                    if (!string.IsNullOrEmpty(prefix) && !fileName.StartsWith(prefix))
                        newName = prefix + newName;
                    if (!string.IsNullOrEmpty(suffix) && !fileName.EndsWith(suffix))
                        newName = newName + suffix;

                    return System.IO.Path.Combine(dir, newName + ext);
                });
            }

            EditorGUILayout.Space();
            EditorGUILayout.Space();

            // --- 模式 2: 字符替换 ---
            GUILayout.Label("模式 B: 字符替换");
            replaceOld = EditorGUILayout.TextField("查找 (Old):", replaceOld);
            replaceNew = EditorGUILayout.TextField("替换为 (New):", replaceNew);

            if (GUILayout.Button("执行替换"))
            {
                RenameSelectedAssets(assetPath =>
                {
                    string dir = System.IO.Path.GetDirectoryName(assetPath);
                    string fileName = System.IO.Path.GetFileName(assetPath); // 带后缀一起换
                
                    if (string.IsNullOrEmpty(replaceOld)) return assetPath;

                    string newName = fileName.Replace(replaceOld, replaceNew);
                    return System.IO.Path.Combine(dir, newName);
                });
            }
        }

        private void RenameSelectedAssets(System.Func<string, string> renameLogic)
        {
            Object[] selectedAssets = Selection.objects;
            if (selectedAssets.Length == 0)
            {
                Debug.LogWarning("请先在 Project 窗口选中要改名的文件！");
                return;
            }

            AssetDatabase.StartAssetEditing();
            foreach (Object obj in selectedAssets)
            {
                string path = AssetDatabase.GetAssetPath(obj);
                if (AssetDatabase.IsValidFolder(path)) continue; // 跳过文件夹

                string newPath = renameLogic(path);
                if (path != newPath)
                {
                    string error = AssetDatabase.MoveAsset(path, newPath);
                    if (!string.IsNullOrEmpty(error))
                        Debug.LogError($"重命名失败: {error}");
                }
            }
            AssetDatabase.StopAssetEditing();
            AssetDatabase.Refresh();
            Debug.Log($"已尝试重命名 {selectedAssets.Length} 个文件。");
        }
    }
}