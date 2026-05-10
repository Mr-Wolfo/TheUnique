using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

[CreateAssetMenu(menuName = "2D/Tiles/Custom Sibling Rule Tile")]
public class SiblingRuleTile : RuleTile {
    
    [Header("Тайлы-друзья")]
    public List<TileBase> siblings = new List<TileBase>();

    public override bool RuleMatch(int neighbor, TileBase other) {
        switch (neighbor) {
            case TilingRuleOutput.Neighbor.This:
                return other == this || siblings.Contains(other);
            
            case TilingRuleOutput.Neighbor.NotThis:
                return other != this && !siblings.Contains(other);
        }
        return base.RuleMatch(neighbor, other);
    }
}