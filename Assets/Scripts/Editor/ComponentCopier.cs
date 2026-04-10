using UnityEditor;
using UnityEditorInternal;
using UnityEngine;
namespace Editor
{
    /// <summary>
    /// 怪物组件一键搬运工具。
    /// <para>功能：将 A 物体的所有组件（跳过 Transform）一键拷贝到 B 物体，并自动修复跨物体的引用问题。</para>
    /// </summary>
    public class ComponentCopier : EditorWindow
    {
        private GameObject _source;
        private GameObject _target;

        [MenuItem("Tools/Enemy Logic Copier (敌方逻辑搬运员)")]
        public static void ShowWindow()
        {
            var window = GetWindow<ComponentCopier>("逻辑搬运员");
            window.minSize = new Vector2(350, 200);
        }

        private void OnGUI()
        {
            GUILayout.Space(10);
            GUILayout.Label("逻辑搬运工具", EditorStyles.boldLabel);
            EditorGUILayout.HelpBox("提示：先将配置完美的怪拖入'源'，再将新模型物体拖入'目标'。点击搬运后，目标物体的同名组件将被覆盖更新，并自动执行同级/子级引用重定向。", MessageType.Info);

            GUILayout.Space(10);
            _source = (GameObject)EditorGUILayout.ObjectField("源对象 (Source)", _source, typeof(GameObject), true);
            _target = (GameObject)EditorGUILayout.ObjectField("目标对象 (Target)", _target, typeof(GameObject), true);

            GUILayout.Space(20);

            GUI.enabled = (_source != null && _target != null);
            if (GUILayout.Button("开始搬运 (Execute Copy)", GUILayout.Height(50)))
            {
                ExecuteBatchCopy();
            }
            GUI.enabled = true;
        }

        private void ExecuteBatchCopy()
        {
            if (_source == _target)
            {
                EditorUtility.DisplayDialog("错误", "源对象和目标对象不能是同一个！", "确认");
                return;
            }

            // 注册撤销记录
            Undo.RegisterCompleteObjectUndo(_target, "Batch Copy Enemy Components");

            // 获取源物体上的所有组件
            Component[] sourceComponents = _source.GetComponents<Component>();
            int copyCount = 0;

            foreach (var srcComp in sourceComponents)
            {
                // 跳过 Transform
                if (srcComp is Transform) continue;

                // 检查目标物体是否已有该组件，如果有，先删除以防重复
                Component existingComp = _target.GetComponent(srcComp.GetType());
                if (existingComp != null)
                {
                    DestroyImmediate(existingComp);
                }

                // 拷贝并粘贴组件
                ComponentUtility.CopyComponent(srcComp);
                ComponentUtility.PasteComponentAsNew(_target);

                copyCount++;
            }

            // 关键步骤：执行引用重定向
            int relinkCount = RelinkReferences(_source, _target);

            // 强制刷新目标物体的 Inspector
            EditorUtility.SetDirty(_target);

            Debug.Log($"<color=green>[Copier]</color> 成功搬运了 {copyCount} 个组件，并自动修复了 {relinkCount} 处组件引用。");
            EditorUtility.DisplayDialog("搬运完成",
                $"已成功克隆 {copyCount} 个组件！\n自动修复了 {relinkCount} 处内部关联引用。\n\n" +
                $"注意：\n如果使用的是 NodeCanvas 的 Bound Graph（嵌入式图），内部节点的特定序列化引用机制可能会导致部分连线不受外层 SerializedObject 控制，依然需要手动检查确认。", "确认");
        }

        /// <summary>
        /// 遍历目标物体上的新组件，自动将指向 Source 的引用重定向到 Target 对应的层级结构中
        /// </summary>
        private int RelinkReferences(GameObject sourceGo, GameObject targetGo)
        {
            int relinkedCount = 0;
            // 获取目标对象身上的所有组件
            Component[] targetComponents = targetGo.GetComponents<Component>();

            foreach (Component targetComp in targetComponents)
            {
                if (targetComp == null || targetComp is Transform) continue;

                SerializedObject so = new SerializedObject(targetComp);
                SerializedProperty prop = so.GetIterator();
                bool hasModified = false;

                // 遍历组件内所有序列化属性
                while (prop.NextVisible(true))
                {
                    // 仅处理对象引用类型的属性
                    if (prop.propertyType == SerializedPropertyType.ObjectReference)
                    {
                        UnityEngine.Object refObj = prop.objectReferenceValue;
                        if (refObj == null) continue;

                        // 1. 如果引用的是 GameObject
                        if (refObj is GameObject go)
                        {
                            if (go.transform.IsChildOf(sourceGo.transform))
                            {
                                string relativePath = GetRelativePath(sourceGo.transform, go.transform);
                                Transform mappedTarget = relativePath == "" ? targetGo.transform : targetGo.transform.Find(relativePath);

                                if (mappedTarget != null)
                                {
                                    prop.objectReferenceValue = mappedTarget.gameObject;
                                    hasModified = true;
                                    relinkedCount++;
                                }
                            }
                        }
                        // 2. 如果引用的是 Component (如 Blackboard, Animator, Collider 等)
                        else if (refObj is Component comp)
                        {
                            if (comp.transform != null && comp.transform.IsChildOf(sourceGo.transform))
                            {
                                string relativePath = GetRelativePath(sourceGo.transform, comp.transform);
                                Transform mappedTarget = relativePath == "" ? targetGo.transform : targetGo.transform.Find(relativePath);

                                if (mappedTarget != null)
                                {
                                    // 在映射到的 Target 节点上寻找同类型的组件
                                    Component targetRefComp = mappedTarget.GetComponent(comp.GetType());
                                    if (targetRefComp != null)
                                    {
                                        prop.objectReferenceValue = targetRefComp;
                                        hasModified = true;
                                        relinkedCount++;
                                    }
                                }
                            }
                        }
                    }
                }

                // 属性有修改时应用
                if (hasModified)
                {
                    so.ApplyModifiedProperties();
                }
            }

            return relinkedCount;
        }

        /// <summary>
        /// 计算子节点相对于根节点的路径
        /// </summary>
        private string GetRelativePath(Transform root, Transform child)
        {
            if (root == child) return "";

            string path = child.name;
            Transform current = child.parent;

            while (current != null && current != root)
            {
                path = current.name + "/" + path;
                current = current.parent;
            }

            // 如果 current 不等于 root，说明 child 根本不是 root 的子节点
            return current == root ? path : null;
        }
    }
}