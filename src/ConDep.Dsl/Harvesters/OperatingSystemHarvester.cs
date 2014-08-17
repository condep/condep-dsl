using System;
using System.Collections;
using System.Linq;
using System.Management.Automation;
using ConDep.Dsl.Config;
using ConDep.Dsl.Logging;
using ConDep.Dsl.Remote;

namespace ConDep.Dsl.Harvesters
{
    internal class OperatingSystemHarvester : IHarvestServerInfo
    {
        public void Harvest(ServerConfig server)
        {
            var psExecutor = new PowerShellExecutor(server) { LoadConDepModule = false };
            var osInfo = @"$perfData = Get-WmiObject win32_perfformatteddata_perfos_system -Property SystemUpTime
$compSystem = Get-WmiObject win32_computersystem -Property Name,SystemType
$os = Get-WmiObject win32_operatingsystem -Property Caption,Version,BuildNumber

$osInfo = @{}
$osInfo.SystemUpTime = $perfData.SystemUpTime
$osInfo.HostName = $compSystem.Name
$osInfo.SystemType = $compSystem.SystemType
$osInfo.Name = $os.Caption
$osInfo.Version = $os.Version
$osInfo.BuildNumber = $os.BuildNumber
$osInfo.ProgramFilesFolder = ${Env:ProgramFiles}
$osInfo.ProgramFilesX86Folder = ${Env:ProgramFiles(x86)}

$regKeys = @()

if(Test-Path HKLM:\Software\Microsoft\Windows\CurrentVersion\Uninstall) {
    $regKeys += 'HKLM:\Software\Microsoft\Windows\CurrentVersion\Uninstall'
}

if(Test-Path HKLM:\Software\Wow6432Node\Microsoft\Windows\CurrentVersion\Uninstall) {
    $regKeys += 'HKLM:\Software\Wow6432Node\Microsoft\Windows\CurrentVersion\Uninstall'
}

if(Test-Path HKCU:\Software\Microsoft\Windows\CurrentVersion\Uninstall) {
    $regKeys += 'HKCU:\Software\Microsoft\Windows\CurrentVersion\Uninstall'
}

$packages = Get-ChildItem -Path $regKeys | Get-ItemProperty | where-object { $_.DisplayName -ne $null } | select-object -Property DisplayName | foreach{$_.DisplayName}
$osInfo.InstalledSoftwarePackages = $packages

return $osInfo
";

            var osInfoResult = psExecutor.Execute(osInfo, logOutput: false).FirstOrDefault();

            if (osInfoResult != null)
            {
                server.GetServerInfo().OperatingSystem = new OperatingSystemInfo
                                                        {
                                                            BuildNumber = Convert.ToInt32(osInfoResult.BuildNumber),
                                                            Name = osInfoResult.Name,
                                                            HostName = osInfoResult.HostName,
                                                            SystemType = osInfoResult.SystemType,
                                                            SystemUpTime = TimeSpan.FromSeconds(Convert.ToDouble(osInfoResult.SystemUpTime)),
                                                            Version = osInfoResult.Version,
                                                            ProgramFilesFolder = osInfoResult.ProgramFilesFolder,
                                                            ProgramFilesX86Folder = osInfoResult.ProgramFilesX86Folder,
                                                            InstalledSoftwarePackages = ((ArrayList)((PSObject)osInfoResult.InstalledSoftwarePackages).BaseObject).Cast<string>().ToArray()
                                                        };

                Logger.Verbose("Output from harvester:");
                Logger.Verbose("BuildNumber     : " + osInfoResult.BuildNumber);
                Logger.Verbose("Name            : " + osInfoResult.Name);
                Logger.Verbose("HostName        : " + osInfoResult.HostName);
                Logger.Verbose("SystemType      : " + osInfoResult.SystemType);
                Logger.Verbose("SystemUpTime    : " + osInfoResult.SystemUpTime);
                Logger.Verbose("Version         : " + osInfoResult.Version);
                Logger.Verbose("ProgramFiles    : " + osInfoResult.ProgramFilesFolder);
                Logger.Verbose("ProgramFilesX86 : " + osInfoResult.ProgramFilesX86Folder);
                Logger.Verbose("InstalledPackages:");
                server.GetServerInfo().OperatingSystem.InstalledSoftwarePackages.ToList().ForEach(x => Logger.Verbose("\t" + x));
            }

        }
    }
}