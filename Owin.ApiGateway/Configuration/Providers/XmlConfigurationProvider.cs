namespace Owin.ApiGateway.Configuration.Providers
{
    using System;
    using System.IO;
    using System.Text;
    using System.Web.Hosting;
    using System.Xml.Serialization;

    using Owin.ApiGateway.Common;

    public class XmlConfigurationProvider : IConfigurationProvider
    {
        private readonly IConfigurationStorageService storageService;

        public XmlConfigurationProvider(IConfigurationStorageService storageService)
        {
            this.storageService = storageService;
        }

        public Configuration Load()
        {
            var s = new XmlSerializer(typeof(Configuration));
            var configurationString = this.storageService.Read();
            using (var sr = new StringReader(configurationString))
            {
                return (Configuration)s.Deserialize(sr);
            }
        }

        public void Save(Configuration configuration)
        {
            var s = new XmlSerializer(typeof(Configuration));
            var sb = new StringBuilder();
            using (var sr = new StringWriter(sb))
            {
                s.Serialize(sr, configuration);
            }

            this.storageService.Write(sb.ToString());
        }
        
        public EventHandler<ConfigurationChangedEventArgs> ConfigurationChangedHandler
        {
            get
            {
                throw new NotImplementedException();
            }
        }
    }
}