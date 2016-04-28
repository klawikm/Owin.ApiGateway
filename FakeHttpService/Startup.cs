namespace FakeHttpService
{
    using Owin;

    public class Startup
    {
        public void Configuration(IAppBuilder app)
        {
            app.UseHttpRequestDumper();
        }
    }
}