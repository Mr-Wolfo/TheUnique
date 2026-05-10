using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "WFC/Biome Config")]
public class BiomeConfigSO : ScriptableObject
{
    public int BiomeCount = 4;
    public List<BiomeDefinition> Biomes;
}

[System.Serializable]
public class BiomeDefinition
{
    public string Name;
    public PatternLibrarySO PatternLibrary;
    public PatternLibrarySO TransitionLibrary;
}