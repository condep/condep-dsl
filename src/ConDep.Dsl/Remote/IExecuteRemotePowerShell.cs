using System;
using System.Collections.Generic;
using System.Management.Automation.Runspaces;
using ConDep.Dsl.Config;

namespace ConDep.Dsl.Remote
{
    public class PowerShellModulesToLoad
    {
        public PowerShellModulesToLoad()
        {
            LoadConDepModule = true;
        }

        public bool LoadConDepModule { get; set; }
        public bool LoadConDepNodeModule { get; set; }
        public bool LoadConDepDotNetLibrary { get; set; }
    }

    public interface IExecuteRemotePowerShell
    {
        bool UseCredSSP { get; set; }
        IEnumerable<dynamic> ExecuteLocal(ServerConfig localServer, string commandOrScript, Action<PowerShellModulesToLoad> modulesToLoad = null, IEnumerable<CommandParameter> parameters = null, bool logOutput = true);
        IEnumerable<dynamic> Execute(ServerConfig server, string commandOrScript, Action<PowerShellModulesToLoad> modulesToLoad = null, IEnumerable<CommandParameter> parameters = null, bool logOutput = true);
    }
}