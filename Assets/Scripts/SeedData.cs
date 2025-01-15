using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "NewSeedSprites", menuName = "Inventory/NewSeedSprites")]
public class SeedSprites : ScriptableObject
{
    public List<Sprite> sprites;      // Indique si l'objet peut être empilé
}
