using System;
using System.Collections.Generic;
using ConDep.Dsl.Config;

namespace ConDep.Dsl.Sequence
{
    public class RemoteConditionalSequence : RemoteSequence
    {
        private readonly Predicate<ServerInfo> _condition;
        private readonly bool _expectedConditionResult;

        public RemoteConditionalSequence(IEnumerable<ServerConfig> servers, ILoadBalance loadBalancer, Predicate<ServerInfo> condition, bool expectedConditionResult)
            : base(servers, loadBalancer)
        {
            _condition = condition;
            _expectedConditionResult = expectedConditionResult;
        }

        //protected override void ExecuteOnServer(ServerConfig server, IReportStatus status, ConDepSettings settings, CancellationToken token)
        //{
        //    Logger.WithLogSection("Deployment", () =>
        //        {
        //            if (_condition(server.GetServerInfo()) == _expectedConditionResult)
        //            {
        //                foreach (var element in _sequence)
        //                {
        //                    IExecuteOnServer elementToExecute = element;
        //                    if (element is CompositeSequence)
        //                        elementToExecute.Execute(server, status, settings, token);
        //                    else
        //                        Logger.WithLogSection(element.Name, () => elementToExecute.Execute(server, status, settings, token));
        //                }
        //            }
        //            else
        //            {
        //                Logger.Info("Condition evaluated to false. Will not execute.");
        //            }
        //        });
        //}

        public override string Name
        {
            get { return "Conditional Remote Operation"; }
        }
    }
}