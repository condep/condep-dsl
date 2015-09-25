using System;
using ConDep.Dsl.Config;
using ConDep.Dsl.Remote;

namespace ConDep.Dsl.Harvesters
{
    internal class DiskHarvester : IHarvestServerInfo
    {
        private readonly PowerShellExecutor _executor;

        public DiskHarvester()
        {
            _executor = new PowerShellExecutor();
        }

        public DiskHarvester(PowerShellExecutor executor)
        {
            _executor = executor;
        }

        public void Harvest(ServerConfig server)
        {
            var diskInfo = @"$disks = Get-WmiObject win32_logicaldisk
$result = @()
foreach($disk in $disks) {
    $diskInfo = @{}
    $diskInfo.DeviceId = $disk.DeviceID
    $diskInfo.Size = $disk.Size
    $diskInfo.FreeSpace = $disk.FreeSpace
    $diskInfo.Name = $disk.Name
    $diskInfo.FileSystem = $disk.FileSystem
    $diskInfo.VolumeName = $disk.VolumeName

    $result += ,@($diskInfo)
}

return $result";

            var diskInfoResult = _executor.Execute(server, diskInfo, mod => mod.LoadConDepModule = false, logOutput: false);
            if (diskInfoResult != null)
            {
                foreach (var disk in diskInfoResult)
                {
                    var d = new DiskInfo
                                {
                                    DeviceId = disk.DeviceId,
                                    SizeInKb = disk.Size == null ? 0 : Convert.ToInt64(disk.Size / 1024),
                                    FreeSpaceInKb = disk.FreeSpace == null ? 0 : Convert.ToInt64(disk.FreeSpace / 1024),
                                    Name = disk.Name,
                                    FileSystem = disk.FileSystem,
                                    VolumeName = disk.VolumeName
                                };
                    server.GetServerInfo().Disks.Add(d);
                }
            }

        }
    }
}