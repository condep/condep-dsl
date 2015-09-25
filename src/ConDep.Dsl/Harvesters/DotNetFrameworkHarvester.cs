using ConDep.Dsl.Config;
using ConDep.Dsl.Remote;

namespace ConDep.Dsl.Harvesters
{
    internal class DotNetFrameworkHarvester : IHarvestServerInfo
    {
        private readonly PowerShellExecutor _executor;

        public DotNetFrameworkHarvester()
        {
            _executor = new PowerShellExecutor();
        }

        public DotNetFrameworkHarvester(PowerShellExecutor executor)
        {
            _executor = executor;
        }

        public void Harvest(ServerConfig server)
        {
            var result = _executor.Execute(server, @"$regKeys = @(
        ""HKLM:\SOFTWARE\Microsoft\NET Framework Setup\NDP\v4\Client"", 
        ""HKLM:\SOFTWARE\Microsoft\NET Framework Setup\NDP\v4\Full"", 
        ""HKLM:\SOFTWARE\Microsoft\NET Framework Setup\NDP\v3.5"",
        ""HKLM:\SOFTWARE\Microsoft\NET Framework Setup\NDP\v3.0"",
        ""HKLM:\Software\Microsoft\NET Framework Setup\NDP\v2.0.50727"",
        ""HKLM:\SOFTWARE\Microsoft\NET Framework Setup\NDP\v1.1.4322""
        )

$result = @()

foreach($regKeyPath in $regKeys) {
    if(test-path $regKeyPath) {

        $regKey = Get-Item $regKeyPath
        $installed = $regKey.GetValue(""Install"")
        if($installed) {
            $dotNetVersion = @{}
            $dotNetVersion.Installed = $installed
            $dotNetVersion.Version = $regKey.GetValue(""Version"")
            $dotNetVersion.ServicePack = $regKey.GetValue(""SP"")
            $dotNetVersion.Release = $regKey.GetValue(""Release"")
            $dotNetVersion.TargetVersion = $regKey.GetValue(""TargetVersion"")
            $dotNetVersion.Client = $regKey.Name.ToLower().EndsWith(""client"")
            $dotNetVersion.Full = $regKey.Name.ToLower().EndsWith(""full"")

            $result += ,@($dotNetVersion)
        }
    }
}

return $result", mod => mod.LoadConDepModule = false, logOutput: false);
            foreach (var element in result)
            {
                server.GetServerInfo().DotNetFrameworks.Add(element);
            }
        }
    }
}