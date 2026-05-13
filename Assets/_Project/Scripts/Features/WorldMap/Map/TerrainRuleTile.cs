using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

[CreateAssetMenu(menuName = "Tiles/Terrain Rule Tile")]
public class TerrainRuleTile : RuleTile<TerrainRuleTile.Neighbor>
{
    [Tooltip("Liste des Rule Tiles considérées comme compatibles (ex: sable, terre, etc.)")]
    public List<RuleTile> compatibleRuleTiles;

    public class Neighbor : RuleTile.TilingRule.Neighbor
    {
        public const int Compatible = 3;
    }

    public override bool RuleMatch(int neighbor, TileBase tile)
    {
        switch (neighbor)
        {
            // THIS = Compatible
            case Neighbor.This:
            case Neighbor.Compatible:
                if (tile == null) return false;
                if (tile == this) return true;

                RuleTile other = tile as RuleTile;
                return other != null && compatibleRuleTiles.Contains(other);

            case Neighbor.NotThis:
                if (tile == null) return true;
                if (tile == this) return false;

                RuleTile otherTile = tile as RuleTile;
                return otherTile == null || !compatibleRuleTiles.Contains(otherTile);
        }

        return base.RuleMatch(neighbor, tile);
    }
}
