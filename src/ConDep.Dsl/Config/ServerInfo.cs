using System;
using System.Collections.Generic;
using System.IO;

namespace ConDep.Dsl.Config
{
    [Serializable]
    public class ServerInfo
    {
        private const string RELATIVE_CONDEP_SCRIPTS_FOLDER = @"PSScripts\ConDep";
        private const string RELATIVE_CONDEPNODE_SCRIPTS_FOLDER = @"PSScripts\ConDepNode";

        public DotNetFrameworks DotNetFrameworks { get; } = new DotNetFrameworks();
        public OperatingSystemInfo OperatingSystem { get; set; }

        public string TempFolderPowerShell { get; set; }

        public string TempFolderDos { get; set; }

        public string ConDepScriptsFolder
        {
            get
            {
                return string.IsNullOrWhiteSpace(TempFolderPowerShell) ? string.Empty : Path.Combine(TempFolderPowerShell, RELATIVE_CONDEP_SCRIPTS_FOLDER);
            }
        }

        public string ConDepScriptsFolderDos
        {
            get
            {
                return string.IsNullOrWhiteSpace(TempFolderDos) ? string.Empty : Path.Combine(TempFolderDos, RELATIVE_CONDEP_SCRIPTS_FOLDER);
            }
        }

        public string ConDepNodeScriptsFolder
        {
            get
            {
                return string.IsNullOrWhiteSpace(TempFolderPowerShell) ? string.Empty : Path.Combine(TempFolderPowerShell, RELATIVE_CONDEPNODE_SCRIPTS_FOLDER);
            }
        }

        public IList<NetworkInfo> Network { get; } = new List<NetworkInfo>();

        public IList<DiskInfo> Disks { get; } = new List<DiskInfo>();
    }
}