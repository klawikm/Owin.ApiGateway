namespace Owin.ApiGateway
{
    using System;

    using Microsoft.Owin.Hosting;

    internal class Program
    {
        private static void Main(string[] args)
        {
            var config = Owin.ApiGateway.Configuration.Configuration.Load();
            var baseUrl = string.Format("http://localhost:{0}/", config.Port);

            using (var server = WebApp.Start<Startup>(new StartOptions(baseUrl)))
            {
                Console.WriteLine("API gateway proxy server is running and listening on {0}. Press Enter to quit.", baseUrl);
                Console.ReadKey();
            }
        }
    }
}