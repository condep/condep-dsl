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

        private readonly DotNetFrameworks _dotNetFrameworks = new DotNetFrameworks();
        private readonly IList<NetworkInfo> _network = new List<NetworkInfo>();
        private readonly IList<DiskInfo> _disks = new List<DiskInfo>(); 

        public DotNetFrameworks DotNetFrameworks { get { return _dotNetFrameworks; } }
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

        public IList<NetworkInfo> Network
        {
            get { return _network; }
        }

        public IList<DiskInfo> Disks
        {
            get { return _disks; }
        }
    }
}