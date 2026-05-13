using UnityEngine;

[CreateAssetMenu(fileName = "NewBow", menuName = "Tools/Bow")]
public class BowData : ToolData
{
    [Tooltip("Dégâts infligés aux ennemis")]
    public int damage = 15;
    
    [Tooltip("Portée d'attaque")]
    public float attackRange = 5f;
    
    [Tooltip("Vitesse de la flèche")]
    public float arrowSpeed = 10f;
    
    [Tooltip("Précision de l'arc (plus c'est bas, plus c'est précis)")]
    public float accuracy = 0.1f;
}
