using System;
using System.Collections.Generic;
using Unity.Collections;
using Unity.Mathematics;

public class StateSnapshot
{
    public int X, Y, ChosenId;
    public Dictionary<int, ulong[]> SavedPossible = new();

    public StateSnapshot(int x, int y, int chosenId, int wordsPerPattern)
    {
        X = x; Y = y; ChosenId = chosenId;
    }

    public void Store(int cellIdx, NativeArray<ulong> gridPossible, int wordsPerPattern)
    {
        if (!SavedPossible.ContainsKey(cellIdx))
        {
            ulong[] copy = new ulong[wordsPerPattern];
            for (int i = 0; i < wordsPerPattern; i++) 
                copy[i] = gridPossible[cellIdx * wordsPerPattern + i];
            SavedPossible[cellIdx] = copy;
        }
    }

    public void StoreFromBurst(int cellIdx, NativeList<ulong> oldValues, int listOffset, int wordsPerPattern)
    {
        if (!SavedPossible.ContainsKey(cellIdx))
        {
            ulong[] copy = new ulong[wordsPerPattern];
            for (int i = 0; i < wordsPerPattern; i++) 
                copy[i] = oldValues[listOffset + i];
            SavedPossible[cellIdx] = copy;
        }
    }

    public void RestoreNative(NativeArray<ulong> gridPossible, NativeArray<int> gridCount)
    {
        foreach (var entry in SavedPossible)
        {
            int cellIdx = entry.Key;
            int wordsPerPattern = entry.Value.Length;
            int offset = cellIdx * wordsPerPattern;

            int count = 0;
            for (int i = 0; i < wordsPerPattern; i++)
            {
                gridPossible[offset + i] = entry.Value[i];
                count += math.countbits(entry.Value[i]);
            }
            gridCount[cellIdx] = count;
        }
    }
}