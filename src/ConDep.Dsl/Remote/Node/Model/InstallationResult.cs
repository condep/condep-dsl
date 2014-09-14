using System;

namespace ConDep.Dsl.Remote.Node.Model
{
    public class InstallationResult
    {
        public DateTime StartedUtc { get; set; }
        public int ExitCode { get; set; }
        public string Message { get; set; }
        public string ErrorMessage { get; set; }
        public string Log { get; set; }
        public bool Success { get; set; }
        public bool AllreadyInstalled { get; set; }
    }
}