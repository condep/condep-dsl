using System;
using System.Collections.Generic;
using System.IO;

namespace ConDep.Dsl.Config
{
    [Serializable]
    public class ServerInfo
    {
        private const string RELATIVE_CONDEP_SCRIPTS_FOLDER = @"PSScripts\ConDep";

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
                return Path.Combine(TempFolderPowerShell, RELATIVE_CONDEP_SCRIPTS_FOLDER);
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