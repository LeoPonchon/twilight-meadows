using UnityEngine;

[CreateAssetMenu(fileName = "NewSword", menuName = "Tools/Sword")]
public class SwordData : ToolData
{
    [Tooltip("Dégâts infligés aux ennemis")]
    public int damage = 25;
    
    [Tooltip("Portée d'attaque")]
    public float attackRange = 1.5f;
    
    [Tooltip("Zone d'attaque (rayon autour du joueur)")]
    public float attackRadius = 1f;
}
