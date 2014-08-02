using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Management.Automation.Runspaces;
using System.Threading;
using ConDep.Dsl.Config;
using ConDep.Dsl.Logging;
using ConDep.Dsl.Resources;

namespace ConDep.Dsl.Remote
{
    public class ConDepNodePublisher : IDisposable
    {
        private readonly byte[] _nodeExe;
        private readonly string _nodeDestPath;
        private readonly string _nodeVersion;
        private readonly string _nodeListenUrl;
        private readonly ConDepSettings _settings;

        public ConDepNodePublisher(byte[] nodeExe, string nodeDestPath, string nodeVersion, string nodeListenUrl, ConDepSettings settings)
        {
            _nodeExe = nodeExe;
            _nodeDestPath = nodeDestPath;
            _nodeVersion = nodeVersion;
            _nodeListenUrl = nodeListenUrl;
            _settings = settings;
        }

        public void Execute(ServerConfig server)
        {
            PublishNodeScript(server);

            var nodeCheckExecutor = new PowerShellExecutor(server);
            var nodeCheckResult = nodeCheckExecutor.Execute(string.Format("Get-ConDepNode \"{0}\"", _nodeDestPath), logOutput: false);

            var conDepResult = nodeCheckResult.Single(psObject => psObject.ConDepResult != null).ConDepResult;

            if(conDepResult.NeedNodeDeployment)
            {
                var parameters = new List<CommandParameter>
                                 {
                                     new CommandParameter("remFile", _nodeDestPath),
                                     new CommandParameter("data", _nodeExe),
                                     new CommandParameter("bytes", _nodeExe.Length)
                                 };

                var executor = new PowerShellExecutor(server);
                var result = executor.Execute("Param([string]$remFile, $data, $bytes)\n  Add-ConDepNode $remFile $data $bytes", parameters: parameters, logOutput: false);

                foreach (var psObject in result)
                {
                    Logger.Info("Result: " + psObject.ToString());

                    if (psObject.ConDepResult != null)
                    {
                        Logger.Info("ConDepResult: " + psObject.ConDepResult);
                    }
                }
            }
            else if (!conDepResult.IsNodeServiceRunning)
            {
                var startServiceExecutor = new PowerShellExecutor(server);
                startServiceExecutor.Execute(string.Format("Start-ConDepNode \"{0}\" \"{1}\"", _nodeDestPath, _nodeListenUrl), logOutput: false);
            }
        }

        private void PublishNodeScript(ServerConfig server)
        {
            var assemblyResources = GetType().Assembly.GetManifestResourceNames();
            var scriptResource = assemblyResources.Single(x => x.EndsWith("ConDepNode.ps1"));
            var moduleResource = assemblyResources.Single(x => x.EndsWith("ConDep.psm1"));

            var scriptFilePath = ConDepResourceFiles.ExtractPowerShellFileFromResource(GetType().Assembly, scriptResource);
            var moduleFilePath = ConDepResourceFiles.ExtractPowerShellFileFromResource(GetType().Assembly, moduleResource);

            var publishScript = @"Param([string]$remFile, $data, $bytes)
    $remFile = $ExecutionContext.InvokeCommand.ExpandString($remFile)

    $dir = Split-Path $remFile

    $dirInfo = [IO.Directory]::CreateDirectory($dir)
    [IO.FileStream]$filestream = [IO.File]::OpenWrite( $remFile )
    $filestream.Write( $data, 0, $bytes )
    $filestream.Close()
";
            var scriptDestPath = Path.Combine(server.GetServerInfo().TempFolderPowerShell, @"PSScripts\ConDep", "ConDepNode.ps1");
            var moduleDestPath = Path.Combine(server.GetServerInfo().TempFolderPowerShell, @"PSScripts\ConDep", "ConDep.psm1");
            var scriptByteArray = File.ReadAllBytes(scriptFilePath);
            var moduleByteArray = File.ReadAllBytes(moduleFilePath);

            var scriptParameters = new List<CommandParameter>
                                 {
                                     new CommandParameter("remFile", scriptDestPath),
                                     new CommandParameter("data", scriptByteArray),
                                     new CommandParameter("bytes", scriptByteArray.Length)
                                 };

            var moduleParameters = new List<CommandParameter>
                                 {
                                     new CommandParameter("remFile", moduleDestPath),
                                     new CommandParameter("data", moduleByteArray),
                                     new CommandParameter("bytes", moduleByteArray.Length)
                                 };

            var scriptExecutor = new PowerShellExecutor(server) { LoadConDepModule = false };
            var moduleExecutor = new PowerShellExecutor(server) { LoadConDepModule = false };

            var scriptResult = scriptExecutor.Execute(publishScript, parameters: scriptParameters, logOutput: false);
            var moduleResult = moduleExecutor.Execute(publishScript, parameters: moduleParameters, logOutput: false);

            foreach (var psObject in scriptResult)
            {
                Logger.Info("Result: " + psObject.ToString());

                if (psObject.ConDepResult != null)
                {
                    Logger.Info("ConDepResult: " + psObject.ConDepResult);
                }
            }

            foreach (var psObject in moduleResult)
            {
                Logger.Info("Result: " + psObject.ToString());

                if (psObject.ConDepResult != null)
                {
                    Logger.Info("ConDepResult: " + psObject.ConDepResult);
                }
            }


        }

        public bool ValidateNode(string nodeListenUrl, string userName, string password)
        {
            var api = new Node.Api(nodeListenUrl, userName, password, _settings.Options.ApiTimout);
            if (!api.Validate())
            {
                Thread.Sleep(1000);
                return api.Validate();
            }
            return true;
        }

        public void Dispose()
        {
            Logger.Info("Disposing!!!");            
        }
    }
}