using UnityEngine;

namespace Model.UI
{
    /// <summary>
    /// UI 视图模型。
    /// <para>通过场景挂载，并在 LifetimeScope 中注入，供 UIManager 读取。</para>
    /// </summary>
    public class UIModel : MonoBehaviour
    {
        [field: Header("全屏界面挂载点")]
        [field: SerializeField] public Transform ViewRoot { get; private set; }
        
        [field: Header("弹窗界面挂载点")]
        [field: SerializeField] public Transform WindowRoot { get; private set; }
    }
}
