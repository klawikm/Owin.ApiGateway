namespace Owin.ApiGateway.Configuration.Providers
{
    using System;
    using System.IO;
    using System.Text;
    using System.Web.Hosting;
    using System.Xml.Serialization;

    public class XmlConfigurationProvider : IConfigurationProvider
    {
        private readonly string configurationFileName;

        public XmlConfigurationProvider(string configurationFileName)
        {
            this.configurationFileName = configurationFileName;

            // fix path is APIGateway is hosted in IIS
            if (HostingEnvironment.IsHosted)
            {
                this.configurationFileName = HostingEnvironment.MapPath("~/" + this.configurationFileName);
            }
        }

        public Configuration Load()
        {
            var s = new XmlSerializer(typeof(Configuration));
            var configurationString = File.ReadAllText(this.configurationFileName);
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

            File.WriteAllText(this.configurationFileName, sb.ToString());
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