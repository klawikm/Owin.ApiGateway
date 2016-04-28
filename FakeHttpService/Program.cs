namespace FakeHttpService
{
    using System;

    using Microsoft.Owin.Hosting;

    internal class Program
    {
        private static void Main(string[] args)
        {
            var baseUrl = "http://localhost:5010/";

            using (var server = WebApp.Start<Startup>(new StartOptions(baseUrl)))
            {
                var finish = false;

                do
                {
                    Console.WriteLine("Fake Http Service was started... Press Enter to quit. Press [c] to clear window.");

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