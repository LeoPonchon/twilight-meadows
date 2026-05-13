using UnityEngine;

[CreateAssetMenu(fileName = "NewSpear", menuName = "Tools/Spear")]
public class SpearData : ToolData
{
    [Tooltip("Dégâts infligés aux ennemis")]
    public int damage = 20;
    
    [Tooltip("Portée d'attaque")]
    public float attackRange = 2f;
}
