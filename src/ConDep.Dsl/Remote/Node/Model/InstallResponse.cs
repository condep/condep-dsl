using System.Collections.Generic;

namespace ConDep.Dsl.Remote.Node.Model
{
    public class InstallResponse
    {
        public UninstallRegKey Package { get; set; }
        public List<Link> Links { get; } = new List<Link>();
        public string TempDirForUpload { get; set; }
    }
}