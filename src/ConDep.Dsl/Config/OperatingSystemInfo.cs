using System;
using System.Collections.Generic;

namespace ConDep.Dsl.Config
{
    [Serializable]
    public class OperatingSystemInfo
    {
        public TimeSpan SystemUpTime { get; set; }
        public string HostName { get; set; }
        public string SystemType { get; set; }
        public string Name { get; set; }
        public string Version { get; set; }
        public int BuildNumber { get; set; }
        public string ProgramFilesFolder { get; set; }
        public string ProgramFilesX86Folder { get; set; }
        public IEnumerable<InstalledSoftwarePackage> InstalledSoftwarePackages { get; set; } 

        public WindowsOperatingSystem OsAsEnum()
        {
            switch (BuildNumber)
            {
                case 6001: return WindowsOperatingSystem.WindowsServer2008;
                case 7600: return WindowsOperatingSystem.WindowsServer2008R2;
                case 7601: return WindowsOperatingSystem.WindowsServer2008R2_SP1;
                case 9200: return WindowsOperatingSystem.WindowsServer2012;
                default: return WindowsOperatingSystem.Unknown;
            }
        }
    }

    public class InstalledSoftwarePackage
    {
        public string DisplayName { get; set; }
        public string DisplayVersion { get; set; }
    }
}