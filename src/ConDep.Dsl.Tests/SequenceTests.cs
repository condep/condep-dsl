using ConDep.Dsl.Builders;
using ConDep.Dsl.Config;
using ConDep.Dsl.Operations.LoadBalancer;
using ConDep.Dsl.SemanticModel;
using ConDep.Dsl.SemanticModel.Sequence;
using NUnit.Framework;

namespace ConDep.Dsl.Tests
{
    [TestFixture]
    public class SequenceTests
    {
        private ExecutionSequenceManager _sequenceManager;
        private SequenceTestApp _app;

        [SetUp]
        public void Setup()
        {
            _sequenceManager = new ExecutionSequenceManager(new DefaultLoadBalancer());
            _app = new SequenceTestApp();
        }

        [Test]
        public void TestThatExecutionSequenceIsValid()
        {
            var config = new ConDepEnvConfig {EnvironmentName = "bogusEnv"};
            var server = new ServerConfig { Name = "jat-web03" };
            config.Servers = new[] { server };

            var settings = new ConDepSettings();
            settings.Config = config;

            var local = new LocalOperationsBuilder(_sequenceManager.NewLocalSequence("Test"), config.Servers);
            //Configure.LocalOperations = local;
            _app.Configure(local, settings);

            var notification = new Notification();
            Assert.That(_sequenceManager.IsValid(notification));
        }
    }

    public class SequenceTestApp : ApplicationArtifact
    {
        public override void Configure(IOfferLocalOperations local, ConDepSettings settings)
        {
            local.HttpGet("http://www.con-dep.net");
            local.ToEachServer(server =>
                {
                    server
                        .Require.IIS();

                    server
                        .ExecuteRemote.PowerShell("ipconfig");
                }
            );
            local.HttpGet("http://blog.torresdal.net");
        }
    }
}