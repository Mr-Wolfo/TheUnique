public class BiomeManager : IBiomeManager
{
    private readonly BiomeDistributor _distributor;

    public BiomeManager(BiomeDistributor distributor)
    {
        _distributor = distributor;
    }

    public BiomeWeight[] GetWeights(int x, int y)
    {
        return _distributor.GetWeights(x, y);
    }
}