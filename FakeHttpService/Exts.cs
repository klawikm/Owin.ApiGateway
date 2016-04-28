namespace FakeHttpService
{
    using Owin;

    public static class Exts
    {
        public static void UseHttpRequestDumper(this IAppBuilder app)
        {
            app.Use<DumperMiddleware>();
        }
    }
}