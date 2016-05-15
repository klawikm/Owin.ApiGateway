namespace Owin.ApiGateway
{
    using System;

    using Microsoft.Owin.Hosting;

    using Owin.ApiGateway.Configuration.Providers;

    internal class Program
    {
        private static void Main(string[] args)
        {
            var config = Owin.ApiGateway.Configuration.Configuration.Load();

            //config.Endpoints[0].Urls.Add("testur1");
            //config.Endpoints[0].Urls.Add("testur3");

            //XmlConfigurationProvider s = new XmlConfigurationProvider("test.xml");
            //s.Save(config);

            var baseUrl = string.Format("http://localhost:{0}/", config.Port);

            using (var server = WebApp.Start<Startup>(new StartOptions(baseUrl)))
            {
                Console.WriteLine("API gateway proxy server is running and listening on {0}. Press Enter to quit.", baseUrl);
                Console.ReadKey();
            }
        }
    }
}