namespace Owin.ApiGateway
{
    using System;
    using System.Collections.Generic;
    using System.Configuration;

    using Microsoft.Owin.Hosting;

    using Owin.ApiGateway.Configuration;
    using Owin.ApiGateway.Configuration.Providers;

    internal class Program
    {
        private static void Main(string[] args)
        {
            var config = Owin.ApiGateway.Configuration.Configuration.Load();

            // var e = config.Endpoints[0];
            //e.Instances = new Instances();
            //e.Instances.Instance = new List<Instance>();
            //e.Instances.Instance.Add(new Instance {Status = InstanceStatuses.Up, Url = "asdasdasd"});

            //config.Routes[0].SoapActionCondition.RequiredSoapActions = new[] { "aaa", "bbbb", "ccc" };
            //var local_fs = new LocalFilesystemConfigurationStorageService("test.xml");
            //XmlConfigurationProvider s = new XmlConfigurationProvider(local_fs);
            //s.Save(config);

            int port = config.Port;
            if (ConfigurationManager.AppSettings["portNumber"] != null)
            {
                port = Int32.Parse(ConfigurationManager.AppSettings["portNumber"]);
                Console.WriteLine("Using port number from App.Config ({0}) not from main configuration ({1}).", port, config.Port);
            }


            var baseUrl = string.Format("http://localhost:{0}/", port);

            using (var server = WebApp.Start<Startup>(new StartOptions(baseUrl)))
            {
                Console.WriteLine("API gateway proxy server is running and listening on {0}. Press Enter to quit.", baseUrl);
                Console.ReadKey();
            }
        }
    }
}