public struct BiomeWeight 
{ 
    public int BiomeIndex; 
    public float Weight; 
}

public interface IBiomeManager
{
    BiomeWeight[] GetWeights(int x, int y);
}