using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class CellContent
{
    public Sprite FloorSprite;
    public Sprite DecorSprite;
    public List<GameObject> Prefabs = new();

    public int GetHash()
    {
        unchecked
        {
            int hash = 17;
            hash = hash * 31 + (FloorSprite != null ? FloorSprite.name.GetHashCode() : 0);
            hash = hash * 31 + (DecorSprite != null ? DecorSprite.name.GetHashCode() : 0);
            
            var names = new List<string>();
            foreach (var p in Prefabs)
            {
                if (p == null) continue;
                string cleanName = p.name;
                int bracketIndex = cleanName.IndexOf(" (");
                if (bracketIndex != -1) cleanName = cleanName.Substring(0, bracketIndex);
                cleanName = cleanName.Replace("(Clone)", "").Trim();
                
                names.Add(cleanName);
            }
            names.Sort();

            foreach (var n in names)
                hash = hash * 31 + n.GetHashCode();
            
            return hash;
        }
    }
}