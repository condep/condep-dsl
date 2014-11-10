using System;
using ConDep.Dsl.Config;

namespace ConDep.Dsl.Sequence
{
    internal interface IManageRemoteSequence : IManageSequence<IExecuteRemotely>
    {
        CompositeSequence NewCompositeSequence(RemoteCompositeOperation operation);
        CompositeSequence NewConditionalCompositeSequence(Predicate<ServerInfo> condition);
        CompositeSequence NewConditionalCompositeSequence(string conditionScript);
        void DryRun();
    }
}