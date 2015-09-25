using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Management.Automation;
using System.Management.Automation.Runspaces;
using System.Security;
using System.Threading;
using ConDep.Dsl.Config;
using ConDep.Dsl.Logging;
using ConDep.Dsl.Validation;

namespace ConDep.Dsl.Remote
{
    public class PowerShellExecutor
    {
        protected const string SHELL_URI = "http://schemas.microsoft.com/powershell/Microsoft.PowerShell";

        public IEnumerable<dynamic> ExecuteLocal(ServerConfig localServer, string commandOrScript, Action<PowerShellModulesToLoad> modulesToLoad = null, IEnumerable<CommandParameter> parameters = null, bool logOutput = true)
        {
            var connectionInfo = new WSManConnectionInfo();
            var modules = new PowerShellModulesToLoad();
            if (modulesToLoad != null)
            {
                modulesToLoad(modules);
            }

            var folders = new RemoteScriptFolders(localServer);
            return ExecuteCommand(commandOrScript, connectionInfo, modules, folders, parameters, logOutput);
        }

        public IEnumerable<dynamic> Execute(ServerConfig server, string commandOrScript, Action<PowerShellModulesToLoad> modulesToLoad = null, IEnumerable<CommandParameter> parameters = null, bool logOutput = true)
        {
            var folders = new RemoteScriptFolders(server);
            var modules = new PowerShellModulesToLoad();
            if (modulesToLoad != null)
            {
                modulesToLoad(modules);
            }

            var remoteCredential = new PSCredential(server.DeploymentUser.UserName, GetPasswordAsSecString(server.DeploymentUser.Password));
            var connectionInfo = new WSManConnectionInfo(server.PowerShell.SSL, server.Name, ResolvePort(server), "/wsman", SHELL_URI,
                                             remoteCredential);

            if (UseCredSSP)
            {
                using (new CredSSPHandler(connectionInfo, server))
                {
                    return ExecuteCommand(commandOrScript, connectionInfo, modules, folders, parameters, logOutput);
                }
            }

            return ExecuteCommand(commandOrScript, connectionInfo, modules, folders, parameters, logOutput);
        }

        public bool UseCredSSP { get; set; }

        private int ResolvePort(ServerConfig server)
        {
            if (server.PowerShell.SSL && server.PowerShell.HttpsPort != null)
            {
                return server.PowerShell.HttpsPort.Value;
            }
                
            if (server.PowerShell.HttpPort != null)
            {
                return server.PowerShell.HttpPort.Value;
            }

            return server.PowerShell.SSL ? 5986 : 5985;
        }

        internal IEnumerable<dynamic> ExecuteCommand(string commandOrScript, WSManConnectionInfo connectionInfo, PowerShellModulesToLoad modules, RemoteScriptFolders folders, IEnumerable<CommandParameter> parameters = null, bool logOutput = true)
        {
            var host = new ConDepPSHost();
            using (var runspace = RunspaceFactory.CreateRunspace(host, connectionInfo))
            {
                runspace.Open();

                var ps = PowerShell.Create();
                ps.Runspace = runspace;

                using (var pipeline = ps.Runspace.CreatePipeline("set-executionpolicy remotesigned -force; $VerbosePreference = 'continue'; $DebugPreference = 'continue'"))
                {
                    ConfigureConDepModule(pipeline, folders, modules);
                    ConfigureConDepNodeModule(pipeline, folders, modules);
                    ConfigureConDepDotNetLibrary(pipeline, folders, modules);

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

        private void ConfigureConDepDotNetLibrary(Pipeline pipeline, RemoteScriptFolders folders, PowerShellModulesToLoad modules)
        {
            if (modules.LoadConDepDotNetLibrary)
            {
                var netLibraryCmd = string.Format(@"Add-Type -Path ""{0}""",
                    Path.Combine(folders.PSTempFolder, "ConDep.Dsl.Remote.Helpers.dll"));
                Logger.Verbose(netLibraryCmd);
                pipeline.Commands.AddScript(netLibraryCmd);
            }
        }

        private void ConfigureConDepNodeModule(Pipeline pipeline, RemoteScriptFolders folders, PowerShellModulesToLoad modules)
        {
            if (modules.LoadConDepNodeModule)
            {
                var conDepNodeModule = string.Format(@"Import-Module {0}", folders.NodeScriptFolder);
                Logger.Verbose(conDepNodeModule);
                pipeline.Commands.AddScript(conDepNodeModule);
            }
        }

        private void ConfigureConDepModule(Pipeline pipeline, RemoteScriptFolders folders, PowerShellModulesToLoad modules)
        {
            if (modules.LoadConDepModule)
            {
                var conDepModule = string.Format(@"Import-Module {0}", folders.ConDepScriptFolder);
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

    internal class RemoteScriptFolders
    {
        public RemoteScriptFolders(ServerConfig server)
        {
            var serverInfo = server.GetServerInfo();
            PSTempFolder = string.IsNullOrWhiteSpace(serverInfo.TempFolderPowerShell) ? string.Empty : serverInfo.TempFolderPowerShell;
            NodeScriptFolder = serverInfo.ConDepNodeScriptsFolder;
            ConDepScriptFolder = serverInfo.ConDepScriptsFolder;
        }

        public string PSTempFolder { get; set; }
        public string NodeScriptFolder { get; set; }
        public string ConDepScriptFolder { get; set; }
    }
}