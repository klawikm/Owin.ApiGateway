namespace Owin.ApiGateway.HealthMonitor
{
    using System;

    using global::Common.Logging;

    public interface IServiceProbe
    {
        void Initialize(Func<Configuration.Configuration> configuartionProvider, ILog logger);
        void Start();

        void Stop();
    }
}
