using System.Collections.Generic;
using System.Management.Automation.Runspaces;

namespace ConDep.Dsl.Remote
{
    public interface IExecuteRemotePowerShell
    {
        bool LoadConDepModule { get; set; }
        bool LoadConDepNodeModule { get; set; }
        bool LoadConDepDotNetLibrary { get; set; }
        bool UseCredSSP { get; set; }
        IEnumerable<dynamic> ExecuteLocal(string commandOrScript, IEnumerable<CommandParameter> parameters = null, bool logOutput = true);
        IEnumerable<dynamic> Execute(string commandOrScript, IEnumerable<CommandParameter> parameters = null, bool logOutput = true);
    }
}