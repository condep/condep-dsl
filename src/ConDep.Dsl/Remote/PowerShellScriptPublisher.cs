using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Management.Automation;
using System.Management.Automation.Runspaces;
using System.Reflection;
using System.Text.RegularExpressions;
using ConDep.Dsl.Config;
using ConDep.Dsl.Logging;
using ConDep.Dsl.PSScripts.ConDep;
using ConDep.Dsl.Remote.Node.Model;
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

        public void PublishScripts(ServerConfig server)
        {
            string localTargetPath = Path.Combine(Path.GetTempPath(), @"PSScripts\ConDep");

            if (Directory.Exists(localTargetPath))
            {
                Directory.Delete(localTargetPath, true);
            }

            SaveConDepScriptModuleResourceToFolder(localTargetPath);
            SaveConDepScriptResourcesToFolder(localTargetPath);
            SaveExternalScriptResourcesToFolder(localTargetPath);
            SaveExecutionPathScriptsToFolder(localTargetPath);

            SyncDir(localTargetPath, server.GetServerInfo().ConDepScriptsFolderDos, server, _settings);
            //foreach (var srcPath in scriptFiles)
            //{
            //    Logger.Verbose("Found script {0} in assembly {1}", srcPath, GetType().Assembly.FullName);
            //    var dstPath = Path.Combine(server.GetServerInfo().ConDepScriptsFolder, Path.GetFileName(srcPath)); 
            //    CopyFile(srcPath, dstPath, server, _settings);
            //}
        }

        private void SaveConDepScriptModuleResourceToFolder(string localTargetPath)
        {
            var resource = ConDepResources.ConDepModule;
            GetFilePathForConDepScriptModule(resource, localTargetPath);
        }

        private string GetFilePathForConDepScriptModule(string resource, string localTargetPath)
        {
            var regex = new Regex(@".+\.(.+\.(ps1|psm1))");
            var match = regex.Match(resource);
            if (match.Success)
            {
                var resourceName = match.Groups[1].Value;
                if (!string.IsNullOrWhiteSpace(resourceName))
                {
                    var resourceNamespace = resource.Replace("." + resourceName, "");
                    return ConDepResourceFiles.GetFilePath(GetType().Assembly, resourceNamespace, resourceName, dirPath: localTargetPath, keepOriginalFileName: true);
                }
            }
            return null;
        }

        public void SyncDir(string srcDir, string dstDir, ServerConfig server, ConDepSettings settings)
        {
            var filePublisher = new FilePublisher();
            filePublisher.PublishDirectory(srcDir, dstDir, server, settings);
        }

        //public void PublishExternalScripts(ServerConfig server)
        //{
        //    var files = SaveExecutionPathScriptsToFolder();
        //    foreach (var file in files)
        //    {
        //        var dstPath = string.Format(@"{0}\PSScripts\ConDep\{1}", server.GetServerInfo().TempFolderDos, Path.GetFileName(file));
        //        CopyFile(file, dstPath, server, _settings);
        //    }
        //}

        public void PublishRemoteHelperAssembly(ServerConfig server)
        {
            var src = Path.Combine(Path.GetDirectoryName(GetType().Assembly.Location), "ConDep.Dsl.Remote.Helpers.dll");
            CopyFile(src, server, _settings);
        }

        private void CopyFile(string srcPath, ServerConfig server, ConDepSettings settings)
        {
            var dstPath = Path.Combine(server.GetServerInfo().TempFolderDos, Path.GetFileName(srcPath));
            CopyFile(srcPath, dstPath, server, settings);
        }

        private void CopyFile(string srcPath, string dstPath, ServerConfig server, ConDepSettings settings)
        {
            var filePublisher = new FilePublisher();
            filePublisher.PublishFile(srcPath, dstPath, server, settings);
        }

        private void SaveExecutionPathScriptsToFolder(string localTargetPath)
        {
            var currDir = Directory.GetCurrentDirectory();
            var dirInfo = new DirectoryInfo(currDir);
            var files = dirInfo.GetFiles("*.ps1", SearchOption.AllDirectories);
            foreach (var file in files.Select(x => x.FullName))
            {
                File.Copy(file, Path.Combine(localTargetPath, Path.GetFileName(file)));
            }
        }

        private void SaveConDepScriptResourcesToFolder(string localTargetPath)
        {
            var files = new List<string>();
            foreach (
                var childAssembly in
                    AppDomain.CurrentDomain.GetAssemblies()
                        .Where(
                            x =>
                                !x.IsDynamic &&
                                x.FullName.StartsWith("ConDep.")))
            {
                files.AddRange(GetResourcesFromAssembly(childAssembly, localTargetPath));
            }
        }

        private void SaveExternalScriptResourcesToFolder(string localTargetPath)
        {
            var files = new List<string>();
            foreach (
                var childAssembly in
                    AppDomain.CurrentDomain.GetAssemblies()
                        .Where(
                            x =>
                                !x.IsDynamic &&
                                !(x.FullName.StartsWith("ConDep.") || x.FullName.StartsWith("System.") || x.FullName.StartsWith("Microsoft.") || x.FullName.StartsWith("mscorlib") )))
            {
                files.AddRange(GetResourcesFromAssembly(childAssembly, localTargetPath));
            }
        }

        private IEnumerable<string> GetResourcesFromAssembly(Assembly assembly, string localTargetPath)
        {
            return assembly.GetManifestResourceNames().Select(resource => ExtractPowerShellFileFromResource(assembly, localTargetPath, resource)).Where(path => !string.IsNullOrWhiteSpace(path));
        }

        private string ExtractPowerShellFileFromResource(Assembly assembly, string localTargetPath, string resource)
        {
            var regex = new Regex(@".+\.(.+\.ps1)");
            var match = regex.Match(resource);
            if (match.Success)
            {
                var resourceName = match.Groups[1].Value;
                if (!string.IsNullOrWhiteSpace(resourceName))
                {
                    var resourceNamespace = resource.Replace("." + resourceName, "");
                    return ConDepResourceFiles.GetFilePath(assembly, resourceNamespace, resourceName, dirPath: localTargetPath, keepOriginalFileName: true);
                }
            }
            return null;
        }

        public void PublishFile(string srcPath, string dstPath, ServerConfig server)
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
    }
}