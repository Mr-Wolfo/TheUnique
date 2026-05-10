using Unity.Burst;
using Unity.Collections;
using Unity.Mathematics;
using Unity.Jobs;
using Unity.Collections.LowLevel.Unsafe;

[BurstCompile(CompileSynchronously = true, OptimizeFor = OptimizeFor.Performance)]
public struct PropagateJob : IJob
{
    public int StartIdx, Width, Height, NumPatterns, WordsPerPattern;
    public NativeArray<ulong> GridPossible;
    public NativeArray<int> GridCount;
    [ReadOnly] public NativeArray<ulong> WordAdjacency;
    [ReadOnly] public NativeArray<ulong> ByteLut;

    public NativeList<int> OutChangedIndices;
    public NativeList<ulong> OutOldValues;

    public unsafe void Execute()
    {
        var stack = new NativeList<int>(128, Allocator.Temp);
        stack.Add(StartIdx);
        ulong* tempUnion = stackalloc ulong[WordsPerPattern];

        while (stack.Length > 0)
        {
            int currIdx = stack[stack.Length - 1];
            stack.RemoveAt(stack.Length - 1);

            int cx = currIdx % Width; int cy = currIdx / Width;

            for (int d = 0; d < 4; d++)
            {
                int nx = cx, ny = cy;
                if (d == 0) ny++; else if (d == 1) ny--; else if (d == 2) nx--; else if (d == 3) nx++;
                if (nx < 0 || nx >= Width || ny < 0 || ny >= Height) continue;
                int nIdx = ny * Width + nx;
                if (GridCount[nIdx] <= 1) continue;

                UnsafeUtility.MemClear(tempUnion, WordsPerPattern * sizeof(ulong));
                int currOffset = currIdx * WordsPerPattern;

                for (int w = 0; w < WordsPerPattern; w++)
                {
                    ulong word = GridPossible[currOffset + w];
                    if (word == 0) continue;

                    if (word == ulong.MaxValue) {
                        int baseW = (d * WordsPerPattern + w) * WordsPerPattern;
                        for (int uw = 0; uw < WordsPerPattern; uw++) tempUnion[uw] |= WordAdjacency[baseW + uw];
                    } else {
                        for (int b = 0; b < 8; b++) {
                            int val = (int)((word >> (b << 3)) & 0xFF);
                            if (val == 0) continue;
                            int lutBase = (((d * WordsPerPattern + w) * 8 + b) * 256 + val) * WordsPerPattern;
                            for (int uw = 0; uw < WordsPerPattern; uw++) tempUnion[uw] |= ByteLut[lutBase + uw];
                        }
                    }
                }

                int nOffset = nIdx * WordsPerPattern;
                bool changed = false;
                for (int w = 0; w < WordsPerPattern; w++) {
                    ulong oldBits = GridPossible[nOffset + w];
                    ulong newBits = oldBits & tempUnion[w];
                    if (oldBits != newBits) {
                        if (!changed) {
                            OutChangedIndices.Add(nIdx);
                            for (int sw = 0; sw < WordsPerPattern; sw++) OutOldValues.Add(GridPossible[nOffset + sw]);
                        }
                        GridPossible[nOffset + w] = newBits;
                        changed = true;
                    }
                }

                if (changed) {
                    int newCount = 0;
                    for (int w = 0; w < WordsPerPattern; w++) newCount += math.countbits(GridPossible[nOffset + w]);
                    GridCount[nIdx] = newCount;
                    if (newCount == 0) return; 
                    stack.Add(nIdx);
                }
            }
        }
    }
}