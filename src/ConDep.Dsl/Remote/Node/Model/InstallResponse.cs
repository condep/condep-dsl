using System.Collections.Generic;

namespace ConDep.Dsl.Remote.Node.Model
{
    public class InstallResponse
    {
        private readonly List<Link> _links = new List<Link>();

        public UninstallRegKey Package { get; set; }
        public List<Link> Links { get { return _links; } }
        public string TempDirForUpload { get; set; }
    }
}