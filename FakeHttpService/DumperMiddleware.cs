namespace FakeHttpService
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Threading.Tasks;

    using AppFunc = System.Func<System.Collections.Generic.IDictionary<string, object>, System.Threading.Tasks.Task>;

    public class DumperMiddleware
    {
        private readonly AppFunc _next;

        private readonly bool silentMode = true;

        public DumperMiddleware(AppFunc next)
        {
            this._next = next;
        }

        public async Task Invoke(IDictionary<string, object> env)
        {
            var inStream = env["owin.RequestBody"] as Stream;
            var outStream = env["owin.ResponseBody"] as Stream;
            var requestHeadersDictionary = env["owin.RequestHeaders"] as IDictionary<string, string[]>;

            if (!this.silentMode)
            {

                Console.WriteLine("----------------------------------------");
                Console.WriteLine("Headers:");
                foreach (var key in requestHeadersDictionary.Keys)
                {
                    var vals = requestHeadersDictionary[key];
                    Console.WriteLine(" {0}", key);

                    foreach (var value in vals)
                    {
                        Console.WriteLine("   {0}", value);
                    }
                }

                Console.WriteLine();
                Console.WriteLine("Env keys:");
                foreach (var key in env.Keys)
                {
                    var val = env[key];
                    Console.WriteLine(" {0} -> {1}", key, val == null ? "<NULL>" : val.ToString());
                }

                Console.WriteLine();
                Console.WriteLine("Input stream:");
                using (var sr = new StreamReader(inStream))
                {
                    var requestString = sr.ReadToEnd();
                    Console.Write(requestString);
                }
            }

            // Send dummy response
            using (var sw = new StreamWriter(outStream))
            {
                sw.Write("<test>123</test>");
            }
        }
    }
}