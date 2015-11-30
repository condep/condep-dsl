using System.Collections.Generic;

namespace ConDep.Dsl.Remote.Node.Model
{
    public class SyncResult
    {
        public List<string> DeletedFiles { get; } = new List<string>();
        public List<string> DeletedDirectories { get; } = new List<string>();
        public List<string> UpdatedFiles { get; } = new List<string>();
        public List<string> CreatedFiles { get; } = new List<string>();
        public List<string> Log { get; } = new List<string>();
        public List<string> Errors { get; } = new List<string>();
    }
}