using UnityEngine;

public abstract class CustomizationObject : ScriptableObject
{
    [Header("Object Info")]
    public string objectName; // Nom de l'objet (ex: "eyes", "long_hair", "short_hair", etc.)
    
    [Header("Sprites")]
    public Sprite[] sprites; // Tous les sprites de cet objet (triés par frame _0, _1, _2, etc.)
    
    [Header("Color Settings")]
    public bool colorable = false; // Si l'objet peut être coloré
    public Color defaultTint = Color.white; // Teinte par défaut
}
