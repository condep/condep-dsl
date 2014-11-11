using System;
using System.Linq;
using System.Threading;
using ConDep.Dsl.Config;
using ConDep.Dsl.Logging;
using ConDep.Dsl.Remote;

namespace ConDep.Dsl.Sequence
{
    internal class CompositeConditionalSequence : CompositeSequence
    {
        internal readonly Predicate<ServerInfo> _condition;
        private readonly bool _expectedConditionResult;
        private readonly string _conditionScript;

        public CompositeConditionalSequence(string name, Predicate<ServerInfo> condition, bool expectedConditionResult)
            : base(name)
        {
            _condition = condition;
            _expectedConditionResult = expectedConditionResult;
        }

        public CompositeConditionalSequence(string name, string conditionScript)
            : base(name)
        {
            _conditionScript = conditionScript;
        }

        public override void Execute(ServerConfig server, IReportStatus status, ConDepSettings settings, CancellationToken token)
        {
            Logger.WithLogSection(Name, () =>
            {
                if (ConditionFulfilled(server))
                {
                    foreach (var element in _sequence)
                    {
                        IExecuteRemotely elementToExecute = element;
                        Logger.WithLogSection("Condition True, executing " + element.Name, () => elementToExecute.Execute(server, status, settings, token));
                    }
                }
                else
                {
                    Logger.Info("Condition evaluated to false. Will not execute.");
                }
            });
        }

        public override string Name
        {
            get { return "Condition"; }
        }

        private bool ConditionFulfilled(ServerConfig server)
        {
            if (string.IsNullOrEmpty(_conditionScript))
            {
                return _condition(server.GetServerInfo()) == _expectedConditionResult;
            }
            else
            {
                var psExecutor = new PowerShellExecutor(server);
                var result = psExecutor.Execute(_conditionScript);
                return result.First().ToString() == "True";
            }
        }
    }
}