using System.Collections;
using System.Collections.Generic;
using UnityEngine.Tilemaps;
using UnityEngine;

public class SiblingRuleTile : RuleTile
{
    [System.Serializable]
    public class SiblingConnection
    {
        public SiblingGroup siblingGroup;
        public bool allowConnection;
    }

    public enum SiblingGroup
    {
        Terrain,
        Ocean,
        Path,
        Moutains,
    }

    public List<SiblingConnection> siblingConnections = new List<SiblingConnection>();

    public override bool RuleMatch(int neighbor, TileBase other)
    {
        if (other is RuleOverrideTile)
            other = (other as RuleOverrideTile).m_InstanceTile;

        switch (neighbor)
        {
            case TilingRule.Neighbor.This:
                {
                    if (other is SiblingRuleTile siblingTile)
                    {
                        foreach (var connection in siblingConnections)
                        {
                            foreach (var otherConnection in siblingTile.siblingConnections)
                            {
                                if (connection.siblingGroup == otherConnection.siblingGroup)
                                {
                                    if (connection.allowConnection)
                                    {
                                        return this;
                                    }
                                    else
                                    {
                                        return other == this;
                                    }
                                }
                            }
                        }
                    }
                    else
                    {
                        return other == this;
                    }
                    return false;
                }

            case TilingRule.Neighbor.NotThis:
                {
                    if (other is SiblingRuleTile siblingTile)
                    {
                        foreach (var connection in siblingConnections)
                        {
                            foreach (var otherConnection in siblingTile.siblingConnections)
                            {
                                if (connection.siblingGroup == otherConnection.siblingGroup)
                                {
                                    if (connection.allowConnection)
                                    {
                                        return false;
                                    }
                                    else
                                    {
                                        return other != this;
                                    }
                                }
                            }
                        }
                    }
                    return true;
                }
        }

        return base.RuleMatch(neighbor, other);
    }
}