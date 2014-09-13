using System;
using ConDep.Dsl.Config;
using ConDep.Dsl.Operations;
using ConDep.Dsl.SemanticModel;

namespace ConDep.Dsl.Sequence
{
    public interface IManageRemoteSequence : IManageSequence<IExecuteRemotely>
    {
        CompositeSequence NewCompositeSequence(RemoteCompositeOperation operation);
        CompositeSequence NewConditionalCompositeSequence(Predicate<ServerInfo> condition);
        void DryRun();
    }
}