using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Management.Automation;
using System.Management.Automation.Runspaces;
using System.Reflection;
using System.Text.RegularExpressions;
using System.Threading;
using ConDep.Dsl.Config;
using ConDep.Dsl.Logging;
using ConDep.Dsl.PSScripts;
using ConDep.Dsl.PSScripts.ConDepNode;
using ConDep.Dsl.Resources;

namespace ConDep.Dsl.Remote
{
    public class ConDepNodePublisher : IDisposable
    {
        private readonly string _srcPath;
        private readonly string _destPath;
        private readonly string _listenUrl;
        private readonly ConDepSettings _settings;

        public ConDepNodePublisher(string srcPath, string destPath, string listenUrl, ConDepSettings settings)
        {
            _srcPath = srcPath;
            _destPath = destPath;
            _listenUrl = listenUrl;
            _settings = settings;
        }

        public void Execute(ServerConfig server)
        {
            DeployNodeScript(server);
            var nodeCheckExecutor = new PowerShellExecutor(server) { LoadConDepModule = false, LoadConDepNodeModule = true };
            var nodeCheckResult = nodeCheckExecutor.Execute(string.Format("Get-ConDepNodeState \"{0}\" \"{1}\"", _destPath, FileHashGenerator.GetFileHash(_srcPath)), logOutput: false);

            var conDepResult = nodeCheckResult.Single(psObject => psObject.ConDepResult != null).ConDepResult;

            if(conDepResult.NeedNodeDeployment)
            {
                var byteArray = File.ReadAllBytes(_srcPath);
                var parameters = new List<CommandParameter>
                                 {
                                     new CommandParameter("path", _destPath),
                                     new CommandParameter("hash", byteArray)
                                 };

                var executor = new PowerShellExecutor(server) { LoadConDepNodeModule = true, LoadConDepModule = false };
                executor.Execute("Param([string]$path, $data)\n  Add-ConDepNode $path $data", parameters: parameters, logOutput: false);
            }
            else if (!conDepResult.IsNodeServiceRunning)
            {
                var startServiceExecutor = new PowerShellExecutor(server);
                startServiceExecutor.Execute(string.Format("Start-ConDepNode \"{0}\" \"{1}\"", _destPath, _listenUrl), logOutput: false);
            }
        }

        private void DeployNodeScript(ServerConfig server)
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

            var scriptByteArray = File.ReadAllBytes(srcPath);

            var scriptParameters = new List<CommandParameter>
            {
                new CommandParameter("path", dstPath),
                new CommandParameter("data", scriptByteArray)
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