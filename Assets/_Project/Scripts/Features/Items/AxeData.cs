using UnityEngine;

[CreateAssetMenu(fileName = "NewAxe", menuName = "Tools/Axe")]
public class AxeData : ToolData
{
    [Tooltip("Dégâts infligés aux arbres")]
    public int damage = 10;
}
