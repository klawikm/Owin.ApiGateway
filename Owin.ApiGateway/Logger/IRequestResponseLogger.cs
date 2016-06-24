namespace Owin.ApiGateway.Logger
{
    public interface IRequestResponseLogger
    {
        void EnqueueLogMessage(LogEntry logEntry);
    }
}
