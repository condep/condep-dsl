using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Management.Automation.Runspaces;
using System.Runtime.InteropServices;
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
        private readonly string _nodeListenUrl;
        private readonly ConDepSettings _settings;

        public ConDepNodePublisher(byte[] nodeExe, string nodeDestPath, string nodeListenUrl, ConDepSettings settings)
        {
            _nodeExe = nodeExe;
            _nodeDestPath = nodeDestPath;
            _nodeListenUrl = nodeListenUrl;
            _settings = settings;
        }

        public void Execute(ServerConfig server)
        {
            var nodeCheckExecutor = new PowerShellExecutor(server);
            var nodeCheckResult = nodeCheckExecutor.Execute(string.Format("Get-ConDepNode \"{0}\" \"{1}\"", _nodeDestPath, GetNodeVersion()), logOutput: false);

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

        private string GetNodeVersion()
        {
            var currentDir = Directory.GetCurrentDirectory();
            var versionInfo = FileVersionInfo.GetVersionInfo(Path.Combine(currentDir, "ConDepNode.exe"));
            return string.Format("{0}.{1}.{2}", versionInfo.FileMajorPart, versionInfo.FileMinorPart, versionInfo.FileBuildPart);
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