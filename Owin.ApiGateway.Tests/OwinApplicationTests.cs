using Microsoft.Owin.Hosting;
using Microsoft.Owin.Testing;
using Owin;
using System;
using System.Net.Http;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Threading.Tasks;

namespace Owin.ApiGateway.Tests
{
    [TestClass]
    public class OwinApplicationTests
    {
        [TestMethod]
        public async Task OwinAppTest()
        {
            using (var server = TestServer.Create<MyStartup>())
            {
                HttpResponseMessage response = await server.HttpClient.GetAsync("/");

                var responseString = await response.Content.ReadAsStringAsync();

                Assert.AreEqual("Hello world using OWIN TestServer", responseString);
            }
        }
    }

    public class MyStartup
    {
        public void Configuration(IAppBuilder app)
        {
            // app.UseErrorPage(); // See Microsoft.Owin.Diagnostics
            // app.UseWelcomePage("/Welcome"); // See Microsoft.Owin.Diagnostics 
            app.Run(async context =>
            {
                await context.Response.WriteAsync("Hello world using OWIN TestServer");
            });
        }
    }
}
