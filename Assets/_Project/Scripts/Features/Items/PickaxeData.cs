using UnityEngine;

[CreateAssetMenu(fileName = "NewPickaxe", menuName = "Tools/Pickaxe")]
public class PickaxeData : ToolData
{
    [Tooltip("Dégâts infligés aux roches")]
    public int damage = 15;
}
