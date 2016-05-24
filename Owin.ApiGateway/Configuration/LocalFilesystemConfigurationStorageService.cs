namespace Owin.ApiGateway.Configuration
{
    using System.IO;

    using Owin.ApiGateway.Common;
    public class LocalFilesystemConfigurationStorageService : IConfigurationStorageService
    {
        private readonly string configurationFileName;

        public LocalFilesystemConfigurationStorageService(string configurationFileName)
        {
            this.configurationFileName = configurationFileName;
        }

        public string Read()
        {
            var configurationString = File.ReadAllText(this.configurationFileName);

            return configurationString;
        }

        public void Write(string configurationString)
        {
            File.WriteAllText(this.configurationFileName, configurationString);
        }
    }
}
