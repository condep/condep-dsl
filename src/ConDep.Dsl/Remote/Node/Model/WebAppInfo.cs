using System.Collections.Generic;

namespace ConDep.Dsl.Remote.Node.Model
{
    public class WebAppInfo
    {
        public List<Link> Links { get; } = new List<Link>();

        public string PhysicalPath { get; set; }

        public bool Exist { get; set; }
    }
}