using System.Collections.Generic;

namespace ConDep.Dsl
{
    public interface IRequireRemotePowerShellScripts
    {
        IEnumerable<string> ScriptPaths { get; }
    }
}