namespace Owin.ApiGateway.BusProvider.Rebus
{
    using System;
    using System.Threading.Tasks;

    using global::Rebus.Handlers;

    using Owin.ApiGateway.Common;

    public class HandlerWrapper<TMessage> : IHandleMessages<TMessage> where TMessage : IMessage
    {
        public Func<TMessage, Task> handler;

        public HandlerWrapper(Func<TMessage, Task> handler)
        {
            this.handler = handler;
        }

        public Task Handle(TMessage message)
        {
            return this.handler(message);
        }
    }
}