using System;
using ConDep.Dsl.Config;

namespace ConDep.Dsl.Sequence
{
    public interface IManageRemoteServersSequence : IManageSequence<IExecuteRemotely>
    {
        IManageRemoteServersSequence NewCompositeServersSequence(RemoteCompositeOperation operation);
        IManageRemoteServersSequence NewConditionalCompositeServersSequence(Predicate<ServerInfo> condition);
        void DryRun();
    }
}