using System.Collections.Generic;

namespace Owin.ApiGateway.Configuration
{
    public class Instances
    {
        public Instances()
        {
            this.Instance = new List<Instance>();
        }

        public List<Instance> Instance { get; set; }
    }
}
