namespace Owin.ApiGateway.CircuitBreaker
{
    using System;

    /// <summary>
    /// Code comes from https://msdn.microsoft.com/en-us/library/dn589784.aspx
    /// </summary>
    interface ICircuitBreakerStateStore
    {
        CircuitBreakerStateEnum State { get; }

        Exception LastException { get; }

        DateTime LastStateChangedDateUtc { get; }

        /// <summary>
        /// The Trip method switches the sate of the circuit breaker to the open state and records the exception that caused the change in state, 
        /// together with the date and time that the exception occurred
        /// </summary>
        /// <param name="ex"></param>
        void Trip(Exception ex);

        void Reset();

        void HalfOpen();

        bool IsClosed { get; }
    }
}
