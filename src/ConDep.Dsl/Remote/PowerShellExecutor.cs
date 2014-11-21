using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Management.Automation;
using System.Management.Automation.Runspaces;
using System.Security;
using ConDep.Dsl.Config;
using ConDep.Dsl.Logging;

namespace ConDep.Dsl.Remote
{
    public class PowerShellExecutor
    {
        private readonly ServerConfig _server;

        protected const string SHELL_URI = "http://schemas.microsoft.com/powershell/Microsoft.PowerShell";

        public PowerShellExecutor(ServerConfig server)
        {
            _server = server;
            LoadConDepModule = true;
        }

        public bool LoadConDepModule { get; set; }
        public bool LoadConDepNodeModule { get; set; }
        public bool LoadConDepDotNetLibrary { get; set; }
        public bool UseCredSSP { get; set; }

        public IEnumerable<dynamic> ExecuteLocal(string commandOrScript, IEnumerable<CommandParameter> parameters = null, bool logOutput = true)
        {
            var connectionInfo = new WSManConnectionInfo();
            return ExecuteCommand(commandOrScript, connectionInfo, parameters, logOutput);
        }

        public IEnumerable<dynamic> Execute(string commandOrScript, IEnumerable<CommandParameter> parameters = null, bool logOutput = true)
        {
            var remoteCredential = new PSCredential(_server.DeploymentUser.UserName, GetPasswordAsSecString(_server.DeploymentUser.Password));
            var connectionInfo = new WSManConnectionInfo(_server.SSL, _server.Name, ResolvePort(_server), "/wsman", SHELL_URI,
                                             remoteCredential);

            if (UseCredSSP)
            {
                using (new CredSSPHandler(connectionInfo, _server))
                {
                    return ExecuteCommand(commandOrScript, connectionInfo, parameters, logOutput);
                }
            }

            return ExecuteCommand(commandOrScript, connectionInfo, parameters, logOutput);
        }

        private int ResolvePort(ServerConfig server)
        {
            if (server.PowerShellPort != null) return server.PowerShellPort.Value;

            return server.SSL ? 5986 : 5985;
        }

        internal IEnumerable<dynamic> ExecuteCommand(string commandOrScript, WSManConnectionInfo connectionInfo, IEnumerable<CommandParameter> parameters = null, bool logOutput = true )
        {
            var host = new ConDepPSHost();
            using (var runspace = RunspaceFactory.CreateRunspace(host, connectionInfo))
            {
                runspace.Open();

                var ps = PowerShell.Create();
                ps.Runspace = runspace;

                using (var pipeline = ps.Runspace.CreatePipeline("set-executionpolicy remotesigned -force"))
                {
                    ConfigureConDepModule(pipeline);
                    ConfigureConDepNodeModule(pipeline);
                    ConfigureConDepDotNetLibrary(pipeline);

                    ConfigureCommand(commandOrScript, parameters, pipeline);

                    var result = pipeline.Invoke();

                    if (pipeline.Error.Count > 0)
                    {
                        var errorCollection = new PowerShellErrors();
                        foreach (var exception in pipeline.Error.NonBlockingRead().OfType<ErrorRecord>())
                        {
                            errorCollection.Add(exception.Exception);
                        }
                        throw errorCollection;
                    }

                    if (logOutput && result.Count > 0)
                    {
                        Logger.WithLogSection("Script output", () =>
                        {
                            foreach (var psObject in result)
                            {
                                Logger.Info(psObject.ToString());
                            }
                        });
                    }

                    return result;
                }
            }
        } 
        private static void ConfigureCommand(string commandOrScript, IEnumerable<CommandParameter> parameters, Pipeline pipeline)
        {
            if (parameters != null)
            {
                var cmd = new Command(commandOrScript, true);
                foreach (var param in parameters)
                {
                    cmd.Parameters.Add(param);
                }
                pipeline.Commands.Add(cmd);
            }
            else
            {
                pipeline.Commands.AddScript(commandOrScript);
            }
            Logger.Verbose(commandOrScript);
        }

        private void ConfigureConDepDotNetLibrary(Pipeline pipeline)
        {
            if (LoadConDepDotNetLibrary)
            {
                var netLibraryCmd = string.Format(@"Add-Type -Path ""{0}""",
                    Path.Combine(_server.GetServerInfo().TempFolderPowerShell, "ConDep.Dsl.Remote.Helpers.dll"));
                Logger.Verbose(netLibraryCmd);
                pipeline.Commands.AddScript(netLibraryCmd);
            }
        }

        private void ConfigureConDepNodeModule(Pipeline pipeline)
        {
            if (LoadConDepNodeModule)
            {
                var conDepNodeModule = string.Format(@"Import-Module {0}", _server.GetServerInfo().ConDepNodeScriptsFolder);
                Logger.Verbose(conDepNodeModule);
                pipeline.Commands.AddScript(conDepNodeModule);
            }
        }

        private void ConfigureConDepModule(Pipeline pipeline)
        {
            if (LoadConDepModule)
            {
                var conDepModule = string.Format(@"Import-Module {0}", _server.GetServerInfo().ConDepScriptsFolder);
                Logger.Verbose(conDepModule);
                pipeline.Commands.AddScript(conDepModule);
            }
        }


        public static SecureString GetPasswordAsSecString(string password)
        {
            var secureString = new SecureString();
            if (!string.IsNullOrWhiteSpace(password))
            {
                password.ToCharArray().ToList().ForEach(secureString.AppendChar);
            }
            return secureString;
        }

    }
}