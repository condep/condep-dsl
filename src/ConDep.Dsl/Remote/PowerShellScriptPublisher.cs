using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Management.Automation.Runspaces;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Text.RegularExpressions;
using ConDep.Dsl.Config;
using ConDep.Dsl.Logging;
using ConDep.Dsl.Resources;

namespace ConDep.Dsl.Remote
{
    public class PowerShellScriptPublisher
    {
        private readonly ConDepSettings _settings;

        public PowerShellScriptPublisher(ConDepSettings settings)
        {
            _settings = settings;
        }

        public void PublishDslScripts(ServerConfig server)
        {
            var scriptFiles = new List<string>();
            if (RemoteScriptsVersionDifferFrom(FileVersionInfo.GetVersionInfo(GetType().Assembly.Location), server))
            {
                var versionInfo = FileVersionInfo.GetVersionInfo(GetType().Assembly.Location);
                PublishVersionFile(server, versionInfo);
                scriptFiles.AddRange(GetType().Assembly.GetManifestResourceNames().Select(resource => ExtractPowerShellFileFromResource(GetType().Assembly, resource)).Where(path => !string.IsNullOrWhiteSpace(path)).ToList());
            }
            foreach (var file in scriptFiles)
            {
                Logger.Verbose("Found script {0} in assembly {1}", file, GetType().Assembly.FullName);
            }
            if (_settings.Options.Assembly != null)
            {
                var artifactScriptFiles = _settings.Options.Assembly.GetManifestResourceNames().Select(resource => ExtractPowerShellFileFromResource(_settings.Options.Assembly, resource)).Where(path => !string.IsNullOrWhiteSpace(path)).ToList();
                foreach (var file in artifactScriptFiles)
                {
                    Logger.Verbose("Found script {0} in assembly {1}", file, _settings.Options.Assembly.FullName);
                }
                scriptFiles = scriptFiles.Union(artifactScriptFiles).ToList();
            }

            foreach (var srcPath in scriptFiles)
            {
                var dstPath = Path.Combine(server.GetServerInfo().ConDepScriptsFolder, Path.GetFileName(srcPath)); 
                PublishFile(srcPath, dstPath, server);
            }
        }

        private void PublishVersionFile(ServerConfig server, FileVersionInfo versionInfo)
        {
            var localVersion = string.Format("{0}.{1}.{2}", versionInfo.FileMajorPart, versionInfo.FileMinorPart, versionInfo.FileBuildPart);
            var srcPath = Path.Combine(Path.GetTempPath(), "version.json");
            var dstPath = Path.Combine(server.GetServerInfo().ConDepScriptsFolder, Path.GetFileName(srcPath));
            File.WriteAllText(srcPath, string.Format("{{\"version\" : \"{0}\"}}", localVersion));
            PublishFile(srcPath, dstPath, server);
        }

        private bool RemoteScriptsVersionDifferFrom(FileVersionInfo versionInfo, ServerConfig server)
        {
            var localVersion = string.Format("{0}.{1}.{2}", versionInfo.FileMajorPart, versionInfo.FileMinorPart, versionInfo.FileBuildPart);
            var remoteVersionFilePath = Path.Combine(server.GetServerInfo().ConDepScriptsFolder, "version.json");
            var remoteVersion = GetRemoteScriptsVersion(server, remoteVersionFilePath);

            return localVersion != remoteVersion;
        }

        private string GetRemoteScriptsVersion(ServerConfig server, string remoteVersionFilePath)
        {
            var versionScript = string.Format(@"
$conDepReturnValues = New-Object PSObject -Property @{{         
    ConDepResult    = New-Object PSObject -Property @{{
		Version = """"
    }}                 
}}                     

$versionPath = ""{0}""
$versionPath = $ExecutionContext.InvokeCommand.ExpandString($versionPath)
if(!(Test-Path $versionPath)) {{
    return $conDepReturnValues
}}

$versionData = Get-Content $versionPath | ConvertFrom-Json
$conDepReturnValues.ConDepResult.Version = $versionData.version
return $conDepReturnValues
", remoteVersionFilePath);

            var scriptExecutor = new PowerShellExecutor(server) { LoadConDepModule = false };
            var scriptResult = scriptExecutor.Execute(versionScript, logOutput: false);

            foreach (var psObject in scriptResult)
            {
                if (psObject.ConDepResult != null)
                {
                    return psObject.ConDepResult.version;
                }
            }
            return "";
        }

        public void PublishFile(string srcPath, ServerConfig server)
        {
            var fileName = Path.GetFileName(srcPath);
            var dst = Path.Combine(server.GetServerInfo().TempFolderPowerShell, fileName);
            PublishFile(srcPath, dst, server);
        }

        private string ExtractPowerShellFileFromResource(Assembly assembly, string resource)
        {
            var regex = new Regex(@".+\.(.+\.(ps1|psm1))");
            var match = regex.Match(resource);
            if (match.Success)
            {
                var resourceName = match.Groups[1].Value;
                if (!string.IsNullOrWhiteSpace(resourceName))
                {
                    var resourceNamespace = resource.Replace("." + resourceName, "");
                    return ConDepResourceFiles.GetFilePath(assembly, resourceNamespace, resourceName, keepOriginalFileName: true, versionFileName: true);
                }
            }
            return null;
        }

        public void PublishFile(string srcPath, string dstPath, ServerConfig server)
        {
            const string publishScript = @"Param([string]$remFile, $data, $bytes)
    $remFile = $ExecutionContext.InvokeCommand.ExpandString($remFile)

    $dir = Split-Path $remFile

    $dirInfo = [IO.Directory]::CreateDirectory($dir)
    [IO.FileStream]$filestream = [IO.File]::OpenWrite( $remFile )
    $filestream.Write( $data, 0, $bytes )
    $filestream.Close()
    write-host ""File $remFile created""
";

            var scriptByteArray = File.ReadAllBytes(srcPath);

            var scriptParameters = new List<CommandParameter>
            {
                new CommandParameter("remFile", dstPath),
                new CommandParameter("data", scriptByteArray),
                new CommandParameter("bytes", scriptByteArray.Length)
            };
            var scriptExecutor = new PowerShellExecutor(server) { LoadConDepModule = false };
            scriptExecutor.Execute(publishScript, parameters: scriptParameters, logOutput: false);
        }
    }
}