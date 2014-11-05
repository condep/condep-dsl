using System.IO;
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
        const string NODE_LISTEN_URL = "http://{0}:{1}/ConDepNode/";

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
                    //Logger.WithLogSection("Copying external scripts", () => scriptPublisher.PublishExternalScripts(server));
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

                    var nodeUrl = string.Format(NODE_LISTEN_URL, server.Name, settings.Options.NodePort);
                    var nodeLocalhostUrl = string.Format(NODE_LISTEN_URL, "localhost", settings.Options.NodePort);

                    var nodePublisher = new ConDepNodePublisher(path, Path.Combine(server.GetServerInfo().OperatingSystem.ProgramFilesFolder, "ConDepNode", Path.GetFileName(path)), nodeLocalhostUrl, settings);
                    nodePublisher.Execute(server);
                    if (!nodePublisher.ValidateNode(nodeUrl, server.DeploymentUser.UserName, server.DeploymentUser.Password))
                    {
                        throw new ConDepNodeValidationException("Unable to make contact with ConDep Node or return content from API.");
                    }

                    Logger.Info(string.Format("ConDep Node successfully validated on {0}", server.Name));
                    Logger.Info(string.Format("Node listening on {0}", nodeUrl));
                });
        }

        public string Name { get { return "Pre-Operation"; } }

        public void DryRun()
        {
            Logger.WithLogSection(Name, () => { });
        }
    }
}