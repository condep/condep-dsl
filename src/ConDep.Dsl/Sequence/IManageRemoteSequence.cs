using System;
using ConDep.Dsl.Config;
using ConDep.Dsl.Operations;
using ConDep.Dsl.Sequence;

namespace ConDep.Dsl.SemanticModel
{
    public interface IManageRemoteSequence : IManageSequence<IExecuteOnServer>
    {
        CompositeSequence NewCompositeSequence(RemoteCompositeOperation operation);
        CompositeSequence NewConditionalCompositeSequence(Predicate<ServerInfo> condition);
        void DryRun();
    }
}