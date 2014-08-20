using System.IO;
using System.Threading;
using ConDep.Dsl.Config;
using ConDep.Dsl.Logging;
using ConDep.Dsl.Remote;
using ConDep.Dsl.Validation;

namespace ConDep.Dsl.Operations
{
    public abstract class RealRemoteOperation : IExecuteOnServer
    {
        public abstract void Execute(ILogForConDep logger);
        public void Execute(ServerConfig server, IReportStatus status, ConDepSettings settings, CancellationToken token)
        {
            var assemblyLocalDir = Path.GetDirectoryName(GetType().Assembly.Location);
            var assemblyRemoteDir = Path.Combine(server.GetServerInfo().TempFolderDos, "Assemblies");

            var publisher = new FilePublisher();
            publisher.PublishDirectory(assemblyLocalDir, assemblyRemoteDir, server, settings);

            var psExecutor = new PowerShellExecutor(server);
            var script = string.Format(@"
add-type -path {0}
$operation = new-object -typename {1}
$logger = new-object -typename {2} -ArgumentList (Get-Host).UI
$operation.Execute($logger)
", Path.Combine(Path.Combine(server.GetServerInfo().TempFolderPowerShell, "Assemblies"), 
 Path.GetFileName(GetType().Assembly.Location)), GetType().FullName,
 typeof(RemotePowerShellLogger).FullName);

            psExecutor.Execute(script);
        }

        public abstract string Name { get; }

        public void DryRun()
        {
        }

        public bool IsValid(Notification notification)
        {
            return true;
        }
    }
}