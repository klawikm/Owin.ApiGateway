namespace Owin.ApiGateway.Logger
{
    public interface IRequestResponseLogStoreWriter
    {
        void SaveLogData(LogEntry logEntry);
    }
}
