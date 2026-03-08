using UnityEngine;

[CreateAssetMenu(menuName = "UISystem/UI Panel Config", fileName = "UI_PanelConfig_")]
public class UIPanelConfigSO : ScriptableObject
{
    public string panelId;
    public UIKind kind;
    public GameObject prefab;
}
