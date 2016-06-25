namespace Owin.ApiGateway.Exceptions
{
    using System;

    [Serializable]
    public class PipelineConfigurationException : Exception
    {
        public PipelineConfigurationException(string message)
            : base(message)
        {
            
        }

    }
}
