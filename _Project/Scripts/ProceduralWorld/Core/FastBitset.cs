using System;
using System.Collections.Generic;

public class FastBitset
{
    public readonly ulong[] Bits;
    public readonly int Length;

    public FastBitset(int length)
    {
        this.Length = length;
        Bits = new ulong[(length + 63) / 64];
    }

    public void SetAll(bool value)
    {
        ulong mask = value ? ulong.MaxValue : 0;
        for (int i = 0; i < Bits.Length; i++) Bits[i] = mask;
        if (value && Length % 64 != 0) Bits[Bits.Length - 1] &= (1UL << (Length % 64)) - 1;
    }

    public void Set(int index, bool value)
    {
        if (value) Bits[index / 64] |= (1UL << (index % 64));
        else Bits[index / 64] &= ~(1UL << (index % 64));
    }

    public bool Get(int index) => (Bits[index / 64] & (1UL << (index % 64))) != 0;

    public void Intersect(FastBitset other)
    {
        for (int i = 0; i < Bits.Length; i++) Bits[i] &= other.Bits[i];
    }

    public void Union(FastBitset other)
    {
        for (int i = 0; i < Bits.Length; i++) Bits[i] |= other.Bits[i];
    }

    public bool IsEmpty()
    {
        for (int i = 0; i < Bits.Length; i++) if (Bits[i] != 0) return false;
        return true;
    }

    public IEnumerable<int> GetSetBits()
    {
        for (int i = 0; i < Bits.Length; i++)
        {
            ulong block = Bits[i];
            while (block != 0)
            {
                int bit = TrailingZeroCount(block);
                yield return (i << 6) + bit; 
                block &= block - 1; 
            }
        }
    }

    public int CountSetBits()
    {
        int count = 0;
        for (int i = 0; i < Bits.Length; i++) count += PopCount(Bits[i]);
        return count;
    }

    public void CopyFrom(FastBitset other) => Array.Copy(other.Bits, Bits, Bits.Length);


    public static int PopCount(ulong v)
    {
        v -= (v >> 1) & 0x5555555555555555UL;
        v = (v & 0x3333333333333333UL) + ((v >> 2) & 0x3333333333333333UL);
        return (int)(((v + (v >> 4)) & 0xF0F0F0F0F0F0F0FUL) * 0x101010101010101UL >> 56);
    }

    public static int TrailingZeroCount(ulong v)
    {
        if (v == 0) return 64;
        int n = 0;
        if ((v & 0xFFFFFFFF) == 0) { n += 32; v >>= 32; }
        if ((v & 0xFFFF) == 0) { n += 16; v >>= 16; }
        if ((v & 0xFF) == 0) { n += 8; v >>= 8; }
        if ((v & 0xF) == 0) { n += 4; v >>= 4; }
        if ((v & 0x3) == 0) { n += 2; v >>= 2; }
        if ((v & 0x1) == 0) { n += 1; }
        return n;
    }
}