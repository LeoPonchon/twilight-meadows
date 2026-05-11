using UnityEngine;

/// <summary>
/// ScriptableObject contenant les données de base d'un NPC
/// </summary>
[CreateAssetMenu(fileName = "NewNPC", menuName = "NPC/NPC Data")]
public class NPCData : ScriptableObject
{
    [Header("Informations de base")]
    [Tooltip("Nom du NPC")]
    public string npcName = "NPC";
    
    [Tooltip("Description du NPC")]
    [TextArea(3, 5)]
    public string description = "Un personnage non-joueur.";
    
    [Header("Dialogue")]
    [Tooltip("Messages de dialogue par défaut")]
    public string[] defaultDialogue = { "Bonjour !", "Comment allez-vous ?" };
}

