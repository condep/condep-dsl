using System;
using ConDep.Dsl.Config;
using ConDep.Dsl.Operations;
using ConDep.Dsl.Sequence;

namespace ConDep.Dsl.SemanticModel
{
    public interface IManageRemoteServersSequence : IManageSequence<IExecuteRemotely>
    {
        CompositeServersSequence NewCompositeServersSequence(RemoteCompositeOperation operation);
        CompositeServersSequence NewConditionalCompositeServersSequence(Predicate<ServerInfo> condition);
        void DryRun();
    }
}