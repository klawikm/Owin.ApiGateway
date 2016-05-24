namespace Owin.ApiGateway
{
    using System;
    using System.Collections.Generic;
    using System.Threading.Tasks;

    using Owin.ApiGateway.Common;
    public class FakeBus : IBus
    {
        public void Initialize(string hostId)
        {
            Console.WriteLine("Initializing bus on {0}", hostId);
        }

        public Task Publish(IMessage eventMessage, Dictionary<string, string> optionalHeaders = null)
        {
            Console.WriteLine("Publishing {0}", eventMessage);

            return Task.FromResult(0);
        }

        public Task Subscribe<TMessage>() where TMessage : IMessage
        {
            Console.WriteLine("Subscribing {0}", typeof(TMessage));

            return Task.FromResult(0);
        }

        public Task Unsubscribe<TMessage>() where TMessage : IMessage
        {
            Console.WriteLine("Unsubscribing {0}", typeof(TMessage));

            return Task.FromResult(0);
        }

        public IBus RegisterHandler<TMessage>(Func<TMessage, Task> handler) where TMessage : IMessage
        {
            Console.WriteLine("Registering handler for {0}", typeof(TMessage));

            return this;
        }
    }
}
