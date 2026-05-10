using System.Collections.Generic;
using UnityEngine;

public class BiomeDistributor : IBiomeManager
{
    private readonly Vector2[] _centers;
    private readonly int _seed;

    public BiomeDistributor(int biomeCount, int seed)
    {
        _seed = seed;
        _centers = new Vector2[biomeCount];
        var rng = new System.Random(_seed);
        for (int i = 0; i < biomeCount; i++)
            _centers[i] = new Vector2(rng.Next(0, 1000), rng.Next(0, 1000));
    }

    public BiomeWeight[] GetWeights(int x, int y)
    {
        var weights = new List<BiomeWeight>();
        float[] distances = new float[_centers.Length];
        float totalInverseDist = 0;

        for (int i = 0; i < _centers.Length; i++)
        {
            float d = Vector2.Distance(new Vector2(x, y), _centers[i]);
            float noise = Mathf.PerlinNoise(x * 0.05f + i * 100, y * 0.05f + i * 100);
            distances[i] = Mathf.Max(0.1f, d * (1.0f - noise * 0.5f));
            totalInverseDist += 1.0f / distances[i];
        }

        for (int i = 0; i < _centers.Length; i++)
        {
            weights.Add(new BiomeWeight { 
                BiomeIndex = i, 
                Weight = (1.0f / distances[i]) / totalInverseDist 
            });
        }
        return weights.ToArray();
    }
}