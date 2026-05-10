public class WFCCell
{
    public FastBitset Possible;
    public int Count => Possible.CountSetBits();
    public bool Collapsed => Count == 1;

    public WFCCell(int totalPatterns)
    {
        Possible = new FastBitset(totalPatterns);
        Possible.SetAll(true);
    }

    public int GetCollapsed()
    {
        if (Possible.CountSetBits() != 1) return -1;
    
        for (int i = 0; i < Possible.Bits.Length; i++)
        {
            ulong block = Possible.Bits[i];
            if (block == 0) continue;
            for (int bit = 0; bit < 64; bit++)
            {
                if ((block & (1UL << bit)) != 0) return i * 64 + bit;
            }
        }
        return -1;
    }
}