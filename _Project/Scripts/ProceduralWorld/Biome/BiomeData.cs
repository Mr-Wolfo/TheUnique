using UnityEngine;

[System.Serializable]
public class BiomeData
{
    public string Name;

    public PatternLibrarySO MainLibrary;
    public PatternLibrarySO TransitionLibrary;

    public float HeightMultiplier;
    public float MoistureMultiplier;
}