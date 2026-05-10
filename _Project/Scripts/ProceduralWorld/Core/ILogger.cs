public interface ILogger
{
    void Log(string msg);
    void Warning(string msg);
    void Error(string msg);
}