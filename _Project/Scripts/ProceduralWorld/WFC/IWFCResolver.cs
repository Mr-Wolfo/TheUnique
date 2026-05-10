using System;
using System.Threading.Tasks;

public interface IWFCResolver
{
    Task<bool> GenerateAsync(int width, int height, IProgress<WFCCell[,]> progress = null);
    WFCCell[,] GetGrid();
}