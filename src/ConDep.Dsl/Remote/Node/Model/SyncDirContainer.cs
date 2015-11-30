using System.Collections.Generic;

namespace ConDep.Dsl.Remote.Node.Model
{
    public class SyncDirContainer
    {
        public List<string> FilesToCopy { get; } = new List<string>();
        public List<string> FilesToDelete { get; } = new List<string>();
        public List<string> FilesToUpdate { get; } = new List<string>();
    }
}