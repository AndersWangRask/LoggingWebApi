namespace LoggingWebApi.Interfaces
{
    public interface ILogEntrySaverFactory
    {
        ILogEntrySaver CreateSaver();
    }
}
