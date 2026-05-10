using UnityEngine;

public class WorldLogger : ILogger
{
    public void Log(string msg) => Debug.Log($"[WORLD] {msg}");
    public void Warning(string msg) => Debug.LogWarning($"[WORLD] {msg}");
    public void Error(string msg) => Debug.LogError($"[WORLD] {msg}");
}