using System.Threading;
using ConDep.Dsl.Config;
using ConDep.Dsl.Logging;
using ConDep.Dsl.Remote;
using ConDep.Dsl.Validation;

namespace ConDep.Dsl.Operations
{
    internal class PostRemoteOps : IExecuteOnServer
    {
        public void Execute(ServerConfig server, IReportStatus status, ConDepSettings settings, CancellationToken token)
        {
            token.ThrowIfCancellationRequested();

            Logger.WithLogSection(string.Format("Stopping ConDepNode on server {0}", server.Name), () =>
            {
                var executor = new PowerShellExecutor(server) {LoadConDepModule = false, LoadConDepNodeModule = true};
                executor.Execute("Stop-ConDepNode", logOutput: false);
            });
        }

        public string Name { get { return "Post Remote Operation"; } }
        public bool IsValid(Notification notification)
        {
            return true;
        }

        public void DryRun()
        {
            Logger.WithLogSection(Name, () => {});
        }
    }
}