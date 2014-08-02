using System.Threading;
using ConDep.Dsl.Config;
using ConDep.Dsl.Logging;
using ConDep.Dsl.Remote;
using ConDep.Dsl.SemanticModel;
using ConDep.Dsl.Validation;

namespace ConDep.Dsl.Operations
{
    internal class PostRemoteOps : IOperateRemote
    {
        public void Execute(ServerConfig server, IReportStatus status, ConDepSettings settings, CancellationToken token)
        {
            token.ThrowIfCancellationRequested();

            Logger.WithLogSection(string.Format("Stopping ConDepNode on server {0}", server.Name), () =>
                {
                    var script = @"add-type -AssemblyName System.ServiceProcess
$service = get-service condepnode

if($service) {{ 
    $service.Stop()
    $service.WaitForStatus([System.ServiceProcess.ServiceControllerStatus]::Stopped)
}} 
";
            var executor = new PowerShellExecutor(server) {LoadConDepModule = false};
                    executor.Execute(script, logOutput: false);
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