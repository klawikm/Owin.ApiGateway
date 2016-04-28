namespace Owin.ApiGateway.Configuration
{
    using System;

    public interface IConfigurationProvider
    {
        EventHandler<ConfigurationChangedEventArgs> ConfigurationChangedHandler { get; }

        Configuration Load();

        void Save(Configuration configuration);
    }
}