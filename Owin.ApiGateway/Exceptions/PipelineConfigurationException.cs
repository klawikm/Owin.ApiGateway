namespace Owin.ApiGateway.Exceptions
{
    using System;

    public class PipelineConfigurationException : Exception
    {
        public PipelineConfigurationException(string message)
            : base(message)
        {
            
        }

    }
}
