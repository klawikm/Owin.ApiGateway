namespace Owin.ApiGateway.Configuration.Providers
{
    using System;
    using System.IO;

    using Owin.ApiGateway.RoutingConditions;

    using YamlDotNet.Serialization;
    using YamlDotNet.Serialization.NamingConventions;

    public class YamlConfigurationProvider : IConfigurationProvider
    {
        private readonly string configurationFileName;

        public YamlConfigurationProvider(string configurationFileName)
        {
            this.configurationFileName = configurationFileName;
        }

        public Configuration Load()
        {
            var deserializer = new Deserializer(namingConvention: new PascalCaseNamingConvention());

            // See: http://57f4dad48e7a4f7cd171c654226feb5a.proxysheep.com/questions/32855949/how-to-deserialise-child-classes
            deserializer.RegisterTagMapping("tag:yaml.org,2002:SoapActionCondition", typeof(SoapActionCondition));
            deserializer.RegisterTagMapping("tag:yaml.org,2002:RequestPathAndQueryCondition", typeof(RequestPathAndQueryCondition));
            deserializer.RegisterTagMapping("tag:yaml.org,2002:AlwaysMatchingCondition", typeof(AlwaysMatchingCondition));

            var configurationString = File.ReadAllText(this.configurationFileName);
            using (var sr = new StringReader(configurationString))
            {
                var configurationFromFile = deserializer.Deserialize<Configuration>(sr);
                return configurationFromFile;
            }
        }

        public void Save(Configuration configuration)
        {
            throw new NotImplementedException();
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