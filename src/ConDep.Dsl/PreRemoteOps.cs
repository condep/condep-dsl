using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading;
using ConDep.Dsl.Config;
using ConDep.Dsl.Logging;
using ConDep.Dsl.Remote;
using ConDep.Dsl.Validation;

namespace ConDep.Dsl
{
    internal class PreRemoteOps : IExecuteRemotely
    {
        const string TMP_FOLDER = @"{0}\temp\ConDep";

        public void Execute(ServerConfig server, IReportStatus status, ConDepSettings settings, CancellationToken token)
        {
            token.ThrowIfCancellationRequested();

            Logger.WithLogSection("Pre-Operations", () =>
                {
                    server.GetServerInfo().TempFolderDos = string.Format(TMP_FOLDER, "%windir%");
                    Logger.Info(string.Format("Dos temp folder is {0}", server.GetServerInfo().TempFolderDos));

                    server.GetServerInfo().TempFolderPowerShell = string.Format(TMP_FOLDER, "$env:windir");
                    Logger.Info(string.Format("PowerShell temp folder is {0}", server.GetServerInfo().TempFolderPowerShell));

                    //var scriptPublisher = new PowerShellScriptPublisher(settings);
                    //Logger.WithLogSection("Copying internal ConDep scripts", () => scriptPublisher.PublishDslScripts(server));

                    PublishConDepNode(server, settings);
                    
                    var scriptPublisher = new PowerShellScriptPublisher(settings);
                    Logger.WithLogSection("Copying external scripts", () => scriptPublisher.PublishScripts(server));
                    Logger.WithLogSection("Copying remote helper assembly", () => scriptPublisher.PublishRemoteHelperAssembly(server));

                    InstallChocolatey(server, settings);
                    //Logger.WithLogSection("Copying external scripts", () => scriptPublisher.PublishExternalScripts(server));
                });
        }

        private void InstallChocolatey(ServerConfig server, ConDepSettings settings)
        {
            Logger.WithLogSection("Installing Chocolatey", () =>
            {
                var psExecutor = new PowerShellExecutor(server);
                var result = psExecutor.Execute(@"
$conDepReturnValues = New-Object PSObject -Property @{         
    ConDepResult    = New-Object PSObject -Property @{
		Installed = $false
        NeedUpdate = $false
    }                 
}    

try {
    if(!((Test-Path $env:ProgramData\chocolatey) -or (Test-Path $env:HOMEDRIVE\chocolatey))) {
        Write-Host 'Chocolatey not found. Installing now.'
        iex ((new-object net.webclient).DownloadString('https://chocolatey.org/install.ps1'))
		$conDepReturnValues.ConDepResult.NeedUpdate = $false
		$conDepReturnValues.ConDepResult.Installed = $true
    }
    else {
		$conDepReturnValues.ConDepResult.NeedUpdate = $true
		$conDepReturnValues.ConDepResult.Installed = $true
        Write-Host 'Chocolatey allready installed.'
    }
    return $conDepReturnValues    
}
catch {
    Write-Warning 'Failed to install Chocolatey! This could break operations depending on Chocolatey.'
    Write-Warning ""Error message: $($_.Exception.Message)""
}
");

                var resultObj = result.SingleOrDefault(psObject => psObject.ConDepResult != null);
                if (resultObj == null) return;
                
                var condepResult = resultObj.ConDepResult;
                if (condepResult == null) return;

                if (condepResult.NeedUpdate)
                {
                    psExecutor.Execute(@"
write-host 'Updating chocolatey'
chocolatey update");
                }
            });
        }

        public bool IsValid(Notification notification)
        {
            return true;
        }

        //private void CopyResourceFiles(Assembly assembly, IEnumerable<string> resources, ServerConfig server, ConDepSettings settings)
        //{
        //    if (resources == null || assembly == null) return;

        //    var scriptPublisher = new PowerShellScriptPublisher(settings);
        //    scriptPublisher.PublishDslScripts(server);
        //}

        private void PublishConDepNode(ServerConfig server, ConDepSettings settings)
        {
            Logger.WithLogSection("Validating ConDepNode", () =>
                {
                    string path;

                    var executionPath = Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), "ConDepNode.exe");
                    if (!File.Exists(executionPath))
                    {
                        var currentPath = Path.Combine(Directory.GetCurrentDirectory(), "ConDepNode.exe");
                        if (!File.Exists(currentPath))
                        {
                            throw new FileNotFoundException("Could not find ConDepNode.exe. Paths tried: \n" +
                                                            executionPath + "\n" + currentPath);
                        }
                        path = currentPath;
                    }
                    else
                    {
                        path = executionPath;
                    }

                    var nodeUrl = new ConDepNodeUrl(server, settings);

                    var nodePublisher = new ConDepNodePublisher(path, Path.Combine(server.GetServerInfo().OperatingSystem.ProgramFilesFolder, "ConDepNode", Path.GetFileName(path)), nodeUrl);
                    nodePublisher.Execute(server);
                    if (!nodePublisher.ValidateNode(nodeUrl, server.DeploymentUser.UserName, server.DeploymentUser.Password, server))
                    {
                        throw new ConDepNodeValidationException("Unable to make contact with ConDep Node or return content from API.");
                    }

                    Logger.Info(string.Format("ConDep Node successfully validated on {0}", server.Name));
                    Logger.Info(string.Format("Node listening on {0}", nodeUrl.ListenUrl));
                });
        }

        public string Name { get { return "Pre-Operation"; } }

        public void DryRun()
        {
            Logger.WithLogSection(Name, () => { });
        }
    }
}