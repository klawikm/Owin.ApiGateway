namespace FakeHttpService
{
    using System;

    using Microsoft.Owin.Hosting;

    internal class Program
    {
        private static void Main(string[] args)
        {
            if (args.Length == 0)
            {
                Console.WriteLine("Lack of required arguments.");
                Console.WriteLine("Usage: {0}.exe [PortNumber]");
                Console.WriteLine("where: ");
                Console.WriteLine(" - [PortNumber] is number of TCP/IP port on which FakeHttpService will be listening");

                return;
            }

            int portNumber;

            if (!Int32.TryParse(args[0], out portNumber))
            {
                Console.WriteLine("Can not parse first argument to int (port number");

                return;
            }

            var baseUrl = string.Format("http://localhost:{0}/", portNumber);

            using (var server = WebApp.Start<Startup>(new StartOptions(baseUrl)))
            {
                var finish = false;

                do
                {
                    Console.WriteLine("Fake Http Service was started. Service is listening on port {0}. Press Enter to quit. Press [c] to clear window.", portNumber);

                    var key = Console.ReadKey();
                    if (key.KeyChar == 'c')
                    {
                        Console.Clear();
                    }
                    else
                    {
                        finish = true;
                    }
                }
                while (!finish);
            }
        }
    }
}