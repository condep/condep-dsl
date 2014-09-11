using System;
using System.Collections.Generic;
using ConDep.Dsl.Builders;
using ConDep.Dsl.Config;
using ConDep.Dsl.Operations;
using ConDep.Dsl.Operations.Application.Deployment.PowerShellScript;
using ConDep.Dsl.SemanticModel;

namespace ConDep.Dsl.Sequence
{
    public class SequenceFactory
    {
        //private readonly List<IExecuteOnServer> _sequence;

        //public SequenceFactory(List<IExecuteOnServer> sequence)
        //{
        //    _sequence = sequence;
        //}

        //public CompositeSequence NewCompositeSequence(RemoteCompositeOperation operation)
        //{
        //    var sequence = new CompositeSequence(operation.Name);
        //    return ConfigureSequence(operation, sequence);
        //}

        //public CompositeSequence NewConditionalCompositeSequence(Predicate<ServerInfo> condition)
        //{
        //    return new CompositeConditionalSequence("Condition", condition, true);
        //}

        //private CompositeSequence ConfigureSequence(RemoteCompositeOperationBase operation, CompositeSequence sequence)
        //{
        //    if (operation is IRequireRemotePowerShellScripts)
        //    {
        //        var scriptOp = new PowerShellScriptDeployOperation(((IRequireRemotePowerShellScripts)operation).ScriptPaths);
        //        scriptOp.Configure(new RemoteCompositeBuilder(sequence));
        //    }

        //    _sequence.Add(sequence);
        //    return sequence;
        //}
    }
}