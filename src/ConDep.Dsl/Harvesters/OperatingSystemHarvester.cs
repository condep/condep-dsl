using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Management.Automation;
using ConDep.Dsl.Config;
using ConDep.Dsl.Logging;
using ConDep.Dsl.Remote;

namespace ConDep.Dsl.Harvesters
{
    internal class OperatingSystemHarvester : IHarvestServerInfo
    {
        private readonly IExecuteRemotePowerShell _executor;

        public OperatingSystemHarvester(IExecuteRemotePowerShell executor)
        {
            _executor = executor;
        }

        public void Harvest(ServerConfig server)
        {
            var osInfo = @"
    $osInfo = @{}

    try {
        $perfData = Get-WmiObject win32_perfformatteddata_perfos_system -Property SystemUpTime
        $osInfo.SystemUpTime = $perfData.SystemUpTime
    }
    catch {
        write-warning 'Failed to retreive SystemUpTime through WMI. Probably because of a bug in Windows/WMI when the server has been running close to a year without reboot.'
    }

    $compSystem = Get-WmiObject win32_computersystem -Property Name,SystemType
    $os = Get-WmiObject win32_operatingsystem -Property Caption,Version,BuildNumber

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

    $packages = Get-ChildItem -Path $regKeys | Get-ItemProperty | where-object { $_.DisplayName -ne $null } | select-object -Property DisplayName,DisplayVersion | foreach{$_.DisplayName + "";"" + $_.DisplayVersion}
    $osInfo.InstalledSoftwarePackages = $packages

    return $osInfo
";

            var osInfoResult = _executor.Execute(server, osInfo, mod => mod.LoadConDepModule = false, logOutput: false).FirstOrDefault();

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
                                                            InstalledSoftwarePackages = ConvertPackagesStringArrayToObjectCollection(((ArrayList)((PSObject)osInfoResult.InstalledSoftwarePackages).BaseObject).Cast<string>().ToArray())
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
                server.GetServerInfo().OperatingSystem.InstalledSoftwarePackages.ToList().ForEach(x => Logger.Verbose("\t" + x.DisplayName + ", version: " + x.DisplayVersion));
            }
        }

        private IEnumerable<InstalledSoftwarePackage> ConvertPackagesStringArrayToObjectCollection(IEnumerable<string> packages)
        {
            var result = new List<InstalledSoftwarePackage>();
            foreach (var package in packages)
            {
                var splitted = package.Split(';');
                result.Add(new InstalledSoftwarePackage { DisplayName = splitted[0], DisplayVersion = splitted[1] ?? "" });
            }
            return result;
        }
    }
}