using System;
using ConDep.Dsl.Config;
using ConDep.Dsl.Operations;

namespace ConDep.Dsl.Sequence
{
    internal interface IManageRemoteServersSequence : IManageSequence<IExecuteRemotely>
    {
        CompositeServersSequence NewCompositeServersSequence(RemoteCompositeOperation operation);
        CompositeServersSequence NewConditionalCompositeServersSequence(Predicate<ServerInfo> condition);
        void DryRun();
    }
}