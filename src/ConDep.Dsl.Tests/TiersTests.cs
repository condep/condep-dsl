using ConDep.Dsl.Config;
using ConDep.Dsl.Execution;
using ConDep.Dsl.LoadBalancer;
using NUnit.Framework;

namespace ConDep.Dsl.Tests
{
    [TestFixture]
    public class TiersTests
    {
        private ConDepSettings _tierSettings;
        private ConDepSettings _serverSettings;

        [SetUp]
        public void Setup()
        {
            _tierSettings = new ConDepSettings
            {
                Config =
                {
                    Tiers = new[]
                    {
                        new TiersConfig
                        {
                            Name = Tier.Web.ToString(),
                            Servers = new[]
                            {
                                new ServerConfig
                                {
                                    Name = "TierServer1"
                                },
                                new ServerConfig
                                {
                                    Name = "TierServer2"
                                }
                            }
                        }
                    }
                },
                Options = { Assembly = typeof(MyArtifactWithTierTag).Assembly, Application = "MyArtifactWithTierTag" }
            };

            _serverSettings = new ConDepSettings
            {
                Config =
                {
                    Servers = new[]
                    {
                        new ServerConfig
                        {
                            Name = "Server1"
                        },
                        new ServerConfig
                        {
                            Name = "Server2"
                        }
                    }
                },
                Options = { Assembly = typeof(MyArtifactWithTierTag).Assembly, Application = "MyArtifactWithTierTag" }
            };
        }

        [Test]
        [ExpectedException(typeof(ConDepNoArtifactTierDefinedException))]
        public void TestThat_ArtifactFailsWhenNotTaggedWithTierForTierConfig()
        {
            var configHandler = new ArtifactConfigurationHandler(new ArtifactHandler(), new ArtifactDependencyHandler(),
                new ServerHandler(), new DefaultLoadBalancer());

            _tierSettings.Options.Application = typeof (MyArtifactWithoutTierTag).Name;
            configHandler.CreateExecutionSequence(_tierSettings);
        }

        [Test]
        public void TestThat_ArtifactSucceedsWhenTaggedWithTierForTierConfig()
        {
            var configHandler = new ArtifactConfigurationHandler(new ArtifactHandler(), new ArtifactDependencyHandler(),
                new ServerHandler(), new DefaultLoadBalancer());

            _tierSettings.Options.Application = typeof(MyArtifactWithTierTag).Name;
            configHandler.CreateExecutionSequence(_tierSettings);
        }
    }

    [Tier(Tier.Web)]
    public class MyArtifactWithTierTag : Artifact.Local
    {
        public override void Configure(IOfferLocalOperations onLocalMachine, ConDepSettings settings)
        {
        }
    }
    public class MyArtifactWithoutTierTag : Artifact.Local
    {
        public override void Configure(IOfferLocalOperations onLocalMachine, ConDepSettings settings)
        {
        }
    }
}