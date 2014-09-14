using System;
using ConDep.Dsl.Config;
using ConDep.Dsl.Operations;

namespace ConDep.Dsl.Sequence
{
    internal interface IManageRemoteSequence : IManageSequence<IExecuteRemotely>
    {
        CompositeSequence NewCompositeSequence(RemoteCompositeOperation operation);
        CompositeSequence NewConditionalCompositeSequence(Predicate<ServerInfo> condition);
        void DryRun();
    }
}