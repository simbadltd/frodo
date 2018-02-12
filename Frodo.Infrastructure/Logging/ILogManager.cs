namespace Frodo.Infrastructure.Logging
{
    public interface ILogManager
    {
        ILogger GetLogger(string name = null);
    }
}