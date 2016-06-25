namespace Owin.ApiGateway.Configuration
{
    using System;

    public interface IConfigurationProvider
    {
        Configuration Load();

        void Save(Configuration configuration);
    }
}