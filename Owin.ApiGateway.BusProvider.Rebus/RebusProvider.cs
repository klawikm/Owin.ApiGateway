namespace Owin.ApiGateway.BusProvider.Rebus
{
    using System;
    using System.Collections.Generic;
    using System.Configuration;
    using System.Threading.Tasks;

    using global::Rebus.Activation;
    using global::Rebus.Config;
    using global::Rebus.Logging;
    using global::Rebus.Persistence.SqlServer;
    using global::Rebus.Routing.TypeBased;
    using global::Rebus.Transport.SqlServer;

    using Owin.ApiGateway.Common;
    using Owin.ApiGateway.Common.Messages;

    using R = global::Rebus;

    public class RebusProvider : IBus, IDisposable
    {
        private static R.Bus.IBus bus;

        private static R.Activation.BuiltinHandlerActivator activator;

        private string ConnectionString
        {
            get
            {
                return ConfigurationManager.ConnectionStrings["RebusProvider"].ConnectionString;
            }
        }

        public void Initialize(string hostId)
        {
            activator = new BuiltinHandlerActivator();

            //bus = Configure.With(activator)
            //.Transport(t => t.UseSqlServer(this.ConnectionString, "AGMessages", "coordinator"))
            //.Routing(r => r.TypeBased().MapAssemblyOf<ConfigurationChanged>("fakeclientsystem"))
            //.Start();

            //bus = Configure.With(activator)
            //.Logging(l => l.ColoredConsole(minLevel: LogLevel.Warn))
            //.Transport(t => t.UseSqlServer(this.ConnectionString, "AGMessages", "fakeclientsystem"))
            //.Routing(r => r.TypeBased().MapAssemblyOf<ConfigurationChanged>("fakeclientsystem"))
            //.Start();

            bus = Configure.With(activator)
            .Logging(l => l.ColoredConsole(minLevel: LogLevel.Warn))
            .Transport(t => t.UseSqlServer(this.ConnectionString, "ApiGatewayMessages", inputQueueName: hostId))
            .Subscriptions(s => s.StoreInSqlServer(this.ConnectionString, "Subscriptions", isCentralized: true))
            .Start();
        }

        public Task Publish(IMessage eventMessage, Dictionary<string, string> optionalHeaders = null)
        {
            return bus.Publish(eventMessage);
        }

        public Task Subscribe<TMessage>() where TMessage : IMessage
        {
            return bus.Subscribe<TMessage>();
        }

        public Task Unsubscribe<TMessage>() where TMessage : IMessage
        {
            return bus.Unsubscribe<TMessage>();
        }

        public IBus RegisterHandler<TMessage>(Func<TMessage, Task> handler) where TMessage : IMessage
        {
            activator.Register(() => new HandlerWrapper<TMessage>(handler));

            return this;
        }

        public void Dispose()
        {
            bus.Dispose();
        }
    }
}
