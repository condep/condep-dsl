using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Management.Automation;
using System.Management.Automation.Runspaces;
using System.Security.Cryptography.X509Certificates;
using System.Text.RegularExpressions;
using System.Threading;
using ConDep.Dsl.Config;
using ConDep.Dsl.Logging;
using ConDep.Dsl.PSScripts.ConDepNode;
using ConDep.Dsl.PSScripts.PfxInstaller;
using ConDep.Dsl.Resources;

namespace ConDep.Dsl.Remote
{
    public class ConDepNodePublisher
    {
        private const string APP_ID = "{bdffda97-dafa-4213-961b-96686a4ce9c2}";
        private const string CERT_PASS = "open source forever!";
        private const string CERT_THUMBPRINT = "03deb9c0ffd578dd6255b52132dcc2dd5612940c";
        private readonly string _srcPath;
        private readonly string _destPath;
        private readonly ConDepNodeUrl _url;
        private readonly int _timeout;
        //private readonly string _listenUrl;

        //private readonly ConDepSettings _settings;

        public ConDepNodePublisher(string srcPath, string destPath, ConDepNodeUrl url, int timeout)
        {
            _srcPath = srcPath;
            _destPath = destPath;
            _url = url;
            _timeout = timeout;
            //_listenUrl = listenUrl;
            //_settings = settings;
        }

        public void Execute(ServerConfig server)
        {
            ConfigureSsl(server);
            DeployNodeModuleScript(server);
            var nodeState = GetNodeState(server);

            if(nodeState.NeedNodeDeployment)
            {
                DeployNode(server);
            }
            else if (!nodeState.IsNodeServiceRunning)
            {
                StartNode(server);
            }
        }

        private void ConfigureSsl(ServerConfig server)
        {
            var resource = PfxInstallerResource.PfxInstallerScript;
            var script = ConDepResourceFiles.GetResourceText(GetType().Assembly, resource);

            var dstPathDos = Path.Combine(server.GetServerInfo().TempFolderDos, "node.con-dep.net.pfx");
            var dstPathPs = Path.Combine(server.GetServerInfo().TempFolderPowerShell, "node.con-dep.net.pfx");

            var certBytes = ConDepResourceFiles.GetResourceBytes(GetType().Assembly,
                new ConDepResource
                {
                    Resource = "node.con-dep.net.pfx",
                    Namespace = typeof (ConDepResourceFiles).Namespace
                });

            var executor = new PowerShellExecutor(server) { LoadConDepModule = false };

            var scriptResult = executor.Execute(string.Format(@"
$conDepReturnValues = New-Object PSObject -Property @{{         
    ConDepResult    = $false 
}}     

$cert = Get-ChildItem Cert:\LocalMachine\My\{0} -ErrorAction SilentlyContinue
$conDepReturnValues.ConDepResult = !($cert -eq $null)
return $conDepReturnValues
", CERT_THUMBPRINT), logOutput:false);

            var certExist = false;
            foreach (var psObject in scriptResult)
            {
                if (psObject.ConDepResult == null) continue;

                if (psObject.ConDepResult)
                {
                    certExist = true;
                }
            }

            if (!certExist)
            {
                Logger.Info("No SSL cert for ConDepNode found. Publishing now.");
                PublishFile(certBytes, dstPathPs, server);

                executor.Execute(script, new List<CommandParameter>
                {
                    new CommandParameter("filePath", dstPathDos),
                    new CommandParameter("password", CERT_PASS),
                });
                var cmd = string.Format(@"
$certThumbprint = ""{1}""
$appId = ""{2}""
netsh http add sslcert ipport=0.0.0.0:{0} certhash=$certThumbprint appid=$appId", _url.Port, CERT_THUMBPRINT, APP_ID);
                executor.Execute(cmd, logOutput: false);
                Logger.Info("SSL cert for ConDepNode published.");
            }
        }

        private void DeployNode(ServerConfig server)
        {
            var byteArray = File.ReadAllBytes(_srcPath);
            var parameters = new List<CommandParameter>
            {
                new CommandParameter("path", _destPath),
                new CommandParameter("data", byteArray),
                new CommandParameter("url", _url.ListenUrl)
            };

            var executor = new PowerShellExecutor(server) {LoadConDepNodeModule = true, LoadConDepModule = false};
            executor.Execute("Param([string]$path, $data, $url)\n  Add-ConDepNode $path $data $url", parameters: parameters,
                logOutput: false);
        }

        private dynamic GetNodeState(ServerConfig server)
        {
            var nodeCheckExecutor = new PowerShellExecutor(server) {LoadConDepModule = false, LoadConDepNodeModule = true};
            var nodeCheckResult =
                nodeCheckExecutor.Execute(
                    string.Format("Get-ConDepNodeState \"{0}\" \"{1}\"", _destPath, FileHashGenerator.GetFileHash(_srcPath)),
                    logOutput: false);

            return nodeCheckResult.Single(psObject => psObject.ConDepResult != null).ConDepResult;
        }

        public static void StartNode(ServerConfig server)
        {
            var startServiceExecutor = new PowerShellExecutor(server) { LoadConDepNodeModule = true, LoadConDepModule = false };
            startServiceExecutor.Execute("Start-ConDepNode", logOutput: false);
        }

        public bool ValidateNode(ConDepNodeUrl url, string userName, string password)
        {
            var api = new Node.Api(url, userName, password, _timeout);
            if (!api.Validate())
            {
                Thread.Sleep(1000);
                return api.Validate();
            }
            return true;
        }

        private void DeployNodeModuleScript(ServerConfig server)
        {
            var resource = ConDepNodeResources.ConDepNodeModule;

            var localModulePath = GetFilePathForConDepScriptModule(resource);
            if (NeedToDeployScript(server, localModulePath))
            {
                Logger.Verbose("Found script {0} in assembly {1}", localModulePath, GetType().Assembly.FullName);
                var dstPath = Path.Combine(server.GetServerInfo().ConDepNodeScriptsFolder, Path.GetFileName(localModulePath));
                PublishFile(localModulePath, dstPath, server);
            }
        }

        private void PublishFile(string srcPath, string dstPath, ServerConfig server)
        {
            PublishFile(File.ReadAllBytes(srcPath), dstPath, server);
        }

        private void PublishFile(byte[] srcBytes, string dstPath, ServerConfig server)
        {
            const string publishScript = @"Param([string]$path, $data)
    $path = $ExecutionContext.InvokeCommand.ExpandString($path)
    $dir = Split-Path $path

    $dirInfo = [IO.Directory]::CreateDirectory($dir)
    if(Test-Path $path) {
        [IO.File]::Delete($path)
    }

    [IO.FileStream]$filestream = [IO.File]::OpenWrite( $path )
    $filestream.Write( $data, 0, $data.Length )
    $filestream.Close()
    write-host ""File $path created""
";

            var scriptParameters = new List<CommandParameter>
            {
                new CommandParameter("path", dstPath),
                new CommandParameter("data", srcBytes)
            };
            var scriptExecutor = new PowerShellExecutor(server) { LoadConDepModule = false };
            scriptExecutor.Execute(publishScript, parameters: scriptParameters, logOutput: false);
        }

        private bool NeedToDeployScript(ServerConfig server, string localFile)
        {
            const string script = @"Param($fileWithHash, $dir)
$dir = $ExecutionContext.InvokeCommand.ExpandString($dir)

$conDepReturnValues = New-Object PSObject -Property @{         
    ConDepResult    = New-Object PSObject -Property @{
		Files = $null
    }                 
}                  

function Get-ConDepFileHash($path) {
    if(Test-Path $path) {
        $md5 = [System.Security.Cryptography.MD5]::Create()
        $hash = [System.BitConverter]::ToString($md5.ComputeHash([System.IO.File]::ReadAllBytes($path)))
        return $hash.Replace(""-"", """")
    }
    else {
        return """"
    }
}

$returnValues = @()

$hash = Get-ConDepFileHash (Join-Path -path $dir -childpath $($fileWithHash.Item1))
$returnValues += @{
	FileName = $fileWithHash.Item1
	IsEqual = ($hash -eq $fileWithHash.Item2)
}

$conDepReturnValues.ConDepResult.Files = $returnValues
return $conDepReturnValues
";
            var scriptExecutor = new PowerShellExecutor(server) { LoadConDepModule = false };

            var scriptParameters = new List<CommandParameter>
            {
                new CommandParameter("fileWithHash", new Tuple<string, string>(Path.GetFileName(localFile), FileHashGenerator.GetFileHash(localFile))),
                new CommandParameter("dir", server.GetServerInfo().ConDepNodeScriptsFolder)
            };

            var scriptResult = scriptExecutor.Execute(script, logOutput: false, parameters: scriptParameters);

            foreach (var psObject in scriptResult)
            {
                if (psObject.ConDepResult == null || psObject.ConDepResult.Files == null) continue;

                var remoteFilesArray = ((PSObject)psObject.ConDepResult.Files).BaseObject as ArrayList;
                var remoteFiles = remoteFilesArray.Cast<dynamic>().Select(remoteFile => remoteFile);

                return remoteFiles.Any(remoteFile => !remoteFile.IsEqual && remoteFile.FileName == Path.GetFileName(localFile));
            }

            return false;
        }

        private string GetFilePathForConDepScriptModule(string resource)
        {
            var regex = new Regex(@".+\.(.+\.(ps1|psm1))");
            var match = regex.Match(resource);
            if (match.Success)
            {
                var resourceName = match.Groups[1].Value;
                if (!string.IsNullOrWhiteSpace(resourceName))
                {
                    var resourceNamespace = resource.Replace("." + resourceName, "");
                    return ConDepResourceFiles.GetFilePath(GetType().Assembly, resourceNamespace, resourceName, keepOriginalFileName: true);
                }
            }
            return null;
        }
    }
}