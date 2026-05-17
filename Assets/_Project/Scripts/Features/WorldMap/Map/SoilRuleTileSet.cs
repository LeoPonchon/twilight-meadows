using UnityEngine;
using UnityEngine.Tilemaps;

[CreateAssetMenu(fileName = "SoilRuleTileSet", menuName = "World/Soil Rule Tile Set")]
public class SoilRuleTileSet : ScriptableObject
{
    [Header("Soil States (RuleTiles or TileBase)")]
    [Tooltip("Terre creusée / dirt placé (état 1)")]
    public TileBase dugTile;

    [Tooltip("Terre retournée / farmland (état 2)")]
    public TileBase tilledTile;

    [Tooltip("Terre retournée mouillée (état 3)")]
    public TileBase tilledWetTile;
}

