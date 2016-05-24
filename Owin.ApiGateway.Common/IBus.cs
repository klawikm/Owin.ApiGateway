namespace Owin.ApiGateway.Common
{
    using System;
    using System.Threading.Tasks;

    /// <summary>
    /// Interface defines abstraction layer for Service Bus component. The API was created based on Rebus API (https://github.com/rebus-org/Rebus).
    /// </summary>
    public interface IBus
    {
        void Initialize(string hostId);

        /// <summary>
        /// Publishes the event message on the topic defined by the assembly-qualified name of the type of the message
        /// </summary>
        /// <param name="eventMessage">
        /// The event message.
        /// </param>
        /// <param name="optionalHeaders">
        /// The optional Headers.
        /// </param>
        /// <returns>
        /// The <see cref="Task"/>.
        /// </returns>
        Task Publish(IMessage eventMessage, System.Collections.Generic.Dictionary<string, string> optionalHeaders = null);

        /// <summary>
        /// Subscribes to the topic defined by the assembly-qualified name of TMessage.
        /// </summary>
        /// <typeparam name="TMessage"></typeparam>
        /// <returns></returns>
        Task Subscribe<TMessage>() where TMessage : IMessage;

        /// <summary>
        /// Unsubscribes from the topic defined by the assembly-qualified name of TMessage.
        /// </summary>
        /// <typeparam name="TMessage"></typeparam>
        /// <returns></returns>
        Task Unsubscribe<TMessage>() where TMessage : IMessage;

        /// <summary>
        /// Sets up an inline handler for messages of type TMessage
        /// </summary>
        /// <typeparam name="TMessage"></typeparam>
        /// <param name="handler"></param>
        /// <returns></returns>
        IBus RegisterHandler<TMessage>(System.Func<TMessage, Task> handler) where TMessage : IMessage;
    }
}
