using System.Threading;
using ConDep.Dsl.Config;
using ConDep.Dsl.Logging;
using ConDep.Dsl.Sequence;
using ConDep.Dsl.Validation;
using NUnit.Framework;

namespace ConDep.Dsl.Tests
{
    //null: Needs to be refactored and tests should get better names
    [TestFixture]
    public class LoadBalancerTests
    {
        private ConDepSettings _settingsStopAfterMarkedServer;
        private ConDepSettings _settingsContinueAfterMarkedServer;
        private ConDepSettings _settingsDefault;
        private CancellationToken _token;

        [SetUp]
        public void Setup()
        {
            var tokenSource = new CancellationTokenSource();
            _token = tokenSource.Token;
            
            new Logger().AutoResolveLogger();
            _settingsStopAfterMarkedServer = new ConDepSettings
            {
                Config =
                {
                    EnvironmentName = "bogus",
                    LoadBalancer = new LoadBalancerConfig
                    {
                        Name = "bogus"
                    }
                },
                Options =
                    {
                        StopAfterMarkedServer = true,
                        SuspendMode = LoadBalancerSuspendMethod.Graceful
                    }
            };

            _settingsContinueAfterMarkedServer = new ConDepSettings
            {
                Config =
                {
                    EnvironmentName = "bogus",
                    LoadBalancer = new LoadBalancerConfig
                    {
                        Name = "bogus"
                    }
                },
                Options =
                {
                    ContinueAfterMarkedServer = true,
                    SuspendMode = LoadBalancerSuspendMethod.Graceful
                }
            };

            _settingsDefault = new ConDepSettings
            {
                Config =
                {
                    EnvironmentName = "bogus",
                    LoadBalancer = new LoadBalancerConfig
                    {
                        Name = "bogus"
                    }
                },
                Options =
                {
                    SuspendMode = LoadBalancerSuspendMethod.Graceful
                }
            };
        }

        [Test]
        public void TestThatStickyLoadBlancingWithOneServerAndManuelTestWorks()
        {
            var server1 = new ServerConfig { Name = "jat-web01" };

            _settingsStopAfterMarkedServer.Config.Servers = new[] { server1 };

            var loadBalancer = new MockLoadBalancer { Mode = LbMode.Sticky };

            var sequnceManager = new ExecutionSequenceManager(_settingsStopAfterMarkedServer.Config.Servers, loadBalancer);
            sequnceManager.NewRemoteSequence("Test");

            var status = new StatusReporter();
            sequnceManager.Execute(status, _settingsStopAfterMarkedServer, _token);

            Assert.That(loadBalancer.OnlineOfflineSequence.Count, Is.EqualTo(1));

            Assert.That(loadBalancer.OnlineOfflineSequence[0].Item1, Is.EqualTo("jat-web01"));
            Assert.That(loadBalancer.OnlineOfflineSequence[0].Item2, Is.EqualTo("offline"));
        }

        [Test]
        public void TestThatStickyLoadBlancingWithOneServerAndContinueWorks()
        {
            var server1 = new ServerConfig { Name = "jat-web01" };

            _settingsContinueAfterMarkedServer.Config.Servers = new[] { server1 };

            var loadBalancer = new MockLoadBalancer { Mode = LbMode.Sticky };

            var sequnceManager = new ExecutionSequenceManager(_settingsContinueAfterMarkedServer.Config.Servers, loadBalancer);
            sequnceManager.NewRemoteSequence("Test");

            var status = new StatusReporter();
            //remoteSequence.Execute(status, _settingsContinueAfterMarkedServer, _token);
            sequnceManager.Execute(status, _settingsContinueAfterMarkedServer, _token);

            Assert.That(loadBalancer.OnlineOfflineSequence.Count, Is.EqualTo(1));

            Assert.That(loadBalancer.OnlineOfflineSequence[0].Item1, Is.EqualTo("jat-web01"));
            Assert.That(loadBalancer.OnlineOfflineSequence[0].Item2, Is.EqualTo("online"));
        }

        [Test]
        public void TestThatRoundRobinLoadBlancingWithOneServerAndManuelTestWorks()
        {
            var server1 = new ServerConfig { Name = "jat-web01" };

            _settingsStopAfterMarkedServer.Config.Servers = new[] { server1 };

            var loadBalancer = new MockLoadBalancer { Mode = LbMode.RoundRobin };

            var sequnceManager = new ExecutionSequenceManager(_settingsStopAfterMarkedServer.Config.Servers, loadBalancer);
            sequnceManager.NewRemoteSequence("Test");

            var status = new StatusReporter();
            //remoteSequence.Execute(status, _settingsStopAfterMarkedServer, _token);
            sequnceManager.Execute(status, _settingsStopAfterMarkedServer, _token);

            Assert.That(loadBalancer.OnlineOfflineSequence.Count, Is.EqualTo(1));

            Assert.That(loadBalancer.OnlineOfflineSequence[0].Item1, Is.EqualTo("jat-web01"));
            Assert.That(loadBalancer.OnlineOfflineSequence[0].Item2, Is.EqualTo("offline"));
        }

        [Test]
        public void TestThatRoundRobinLoadBlancingWithOneServerAndContinueWorks()
        {
            var server1 = new ServerConfig { Name = "jat-web01" };

            _settingsContinueAfterMarkedServer.Config.Servers = new[] { server1 };

            var loadBalancer = new MockLoadBalancer { Mode = LbMode.RoundRobin };

            var sequnceManager = new ExecutionSequenceManager(_settingsContinueAfterMarkedServer.Config.Servers, loadBalancer);
            sequnceManager.NewRemoteSequence("Test");

            var status = new StatusReporter();
            //remoteSequence.Execute(status, _settingsContinueAfterMarkedServer, _token);
            sequnceManager.Execute(status, _settingsContinueAfterMarkedServer, _token);

            Assert.That(loadBalancer.OnlineOfflineSequence.Count, Is.EqualTo(1));

            Assert.That(loadBalancer.OnlineOfflineSequence[0].Item1, Is.EqualTo("jat-web01"));
            Assert.That(loadBalancer.OnlineOfflineSequence[0].Item2, Is.EqualTo("online"));
        }


        [Test]
        public void TestThatRoundRobinLoadBlancingWithOneServerWorks()
        {
            var server1 = new ServerConfig { Name = "jat-web01" };

            _settingsDefault.Config.Servers = new[] { server1 };

            var loadBalancer = new MockLoadBalancer { Mode = LbMode.RoundRobin };

            var sequnceManager = new ExecutionSequenceManager(_settingsDefault.Config.Servers, loadBalancer);
            sequnceManager.NewRemoteSequence("Test");

            var status = new StatusReporter();
            //remoteSequence.Execute(status, _settingsDefault, _token);
            sequnceManager.Execute(status, _settingsDefault, _token);

            Assert.That(loadBalancer.OnlineOfflineSequence.Count, Is.EqualTo(2));

            Assert.That(loadBalancer.OnlineOfflineSequence[0].Item1, Is.EqualTo("jat-web01"));
            Assert.That(loadBalancer.OnlineOfflineSequence[0].Item2, Is.EqualTo("offline"));

            Assert.That(loadBalancer.OnlineOfflineSequence[1].Item1, Is.EqualTo("jat-web01"));
            Assert.That(loadBalancer.OnlineOfflineSequence[1].Item2, Is.EqualTo("online"));
        }

        [Test]
        public void TestThatStickyLoadBlancingWithOneServerWorks()
        {
            var server1 = new ServerConfig { Name = "jat-web01" };

            _settingsDefault.Config.Servers = new[] { server1 };

            var loadBalancer = new MockLoadBalancer { Mode = LbMode.Sticky };

            var sequnceManager = new ExecutionSequenceManager(_settingsDefault.Config.Servers, loadBalancer);
            sequnceManager.NewRemoteSequence("Test");

            var status = new StatusReporter();
            //remoteSequence.Execute(status, _settingsDefault, _token);
            sequnceManager.Execute(status, _settingsDefault, _token);
            Assert.That(loadBalancer.OnlineOfflineSequence.Count, Is.EqualTo(_settingsDefault.Config.Servers.Count * 2));

            Assert.That(loadBalancer.OnlineOfflineSequence[0].Item1, Is.EqualTo("jat-web01"));
            Assert.That(loadBalancer.OnlineOfflineSequence[0].Item2, Is.EqualTo("offline"));

            Assert.That(loadBalancer.OnlineOfflineSequence[1].Item1, Is.EqualTo("jat-web01"));
            Assert.That(loadBalancer.OnlineOfflineSequence[1].Item2, Is.EqualTo("online"));
        }

        [Test]
        public void TestThatRoundRobinLoadBalancingGoesOnlineOfflineInCorrectOrder()
        {
            var server1 = new ServerConfig { Name = "jat-web01" };
            var server2 = new ServerConfig { Name = "jat-web02" };
            var server3 = new ServerConfig { Name = "jat-web03" };
            var server4 = new ServerConfig { Name = "jat-web04" };
            var server5 = new ServerConfig { Name = "jat-web05" };

            _settingsDefault.Config.Servers = new[] { server1, server2, server3, server4, server5 };

            var loadBalancer = new MockLoadBalancer { Mode = LbMode.RoundRobin };

            var sequnceManager = new ExecutionSequenceManager(_settingsDefault.Config.Servers, loadBalancer);
            sequnceManager.NewRemoteSequence("Test");

            var status = new StatusReporter();
            //remoteSequence.Execute(status, _settingsDefault, _token);
            sequnceManager.Execute(status, _settingsDefault, _token);


            Assert.That(loadBalancer.OnlineOfflineSequence.Count, Is.EqualTo(_settingsDefault.Config.Servers.Count * 2));

            Assert.That(loadBalancer.OnlineOfflineSequence[0].Item1, Is.EqualTo("jat-web01"));
            Assert.That(loadBalancer.OnlineOfflineSequence[0].Item2, Is.EqualTo("offline"));

            Assert.That(loadBalancer.OnlineOfflineSequence[1].Item1, Is.EqualTo("jat-web02"));
            Assert.That(loadBalancer.OnlineOfflineSequence[1].Item2, Is.EqualTo("offline"));

            Assert.That(loadBalancer.OnlineOfflineSequence[2].Item1, Is.EqualTo("jat-web03"));
            Assert.That(loadBalancer.OnlineOfflineSequence[2].Item2, Is.EqualTo("offline"));

            Assert.That(loadBalancer.OnlineOfflineSequence[3].Item1, Is.EqualTo("jat-web01"));
            Assert.That(loadBalancer.OnlineOfflineSequence[3].Item2, Is.EqualTo("online"));

            Assert.That(loadBalancer.OnlineOfflineSequence[4].Item1, Is.EqualTo("jat-web02"));
            Assert.That(loadBalancer.OnlineOfflineSequence[4].Item2, Is.EqualTo("online"));

            Assert.That(loadBalancer.OnlineOfflineSequence[5].Item1, Is.EqualTo("jat-web03"));
            Assert.That(loadBalancer.OnlineOfflineSequence[5].Item2, Is.EqualTo("online"));

            Assert.That(loadBalancer.OnlineOfflineSequence[6].Item1, Is.EqualTo("jat-web04"));
            Assert.That(loadBalancer.OnlineOfflineSequence[6].Item2, Is.EqualTo("offline"));

            Assert.That(loadBalancer.OnlineOfflineSequence[7].Item1, Is.EqualTo("jat-web05"));
            Assert.That(loadBalancer.OnlineOfflineSequence[7].Item2, Is.EqualTo("offline"));

            Assert.That(loadBalancer.OnlineOfflineSequence[8].Item1, Is.EqualTo("jat-web04"));
            Assert.That(loadBalancer.OnlineOfflineSequence[8].Item2, Is.EqualTo("online"));

            Assert.That(loadBalancer.OnlineOfflineSequence[9].Item1, Is.EqualTo("jat-web05"));
            Assert.That(loadBalancer.OnlineOfflineSequence[9].Item2, Is.EqualTo("online"));
        }

        [Test]
        public void TestThatStickyLoadBalancingGoesOnlineOfflineInCorrectOrder()
        {
            var server1 = new ServerConfig { Name = "jat-web01" };
            var server2 = new ServerConfig { Name = "jat-web02" };
            var server3 = new ServerConfig { Name = "jat-web03" };
            var server4 = new ServerConfig { Name = "jat-web04" };
            var server5 = new ServerConfig { Name = "jat-web05" };

            _settingsDefault.Config.Servers = new[] { server1, server2, server3, server4, server5 };

            var loadBalancer = new MockLoadBalancer { Mode = LbMode.Sticky };

            var sequnceManager = new ExecutionSequenceManager(_settingsDefault.Config.Servers, loadBalancer);
            sequnceManager.NewRemoteSequence("Test");

            var status = new StatusReporter();
            //remoteSequence.Execute(status, _settingsDefault, _token);
            sequnceManager.Execute(status, _settingsDefault, _token);

            Assert.That(loadBalancer.OnlineOfflineSequence.Count, Is.EqualTo(_settingsDefault.Config.Servers.Count * 2));

            var serverNumber = 1;
            for (int i = 0; i < loadBalancer.OnlineOfflineSequence.Count; i += 2)
            {
                Assert.That(loadBalancer.OnlineOfflineSequence[i].Item1, Is.EqualTo("jat-web0" + serverNumber));
                Assert.That(loadBalancer.OnlineOfflineSequence[i].Item2, Is.EqualTo("offline"));

                Assert.That(loadBalancer.OnlineOfflineSequence[i + 1].Item1, Is.EqualTo("jat-web0" + serverNumber));
                Assert.That(loadBalancer.OnlineOfflineSequence[i + 1].Item2, Is.EqualTo("online"));
                serverNumber++;
            }
        }

        [Test]
        public void TestThatRoundRobinWithManualTestStopsAfterFirstServer()
        {
            var server1 = new ServerConfig { Name = "jat-web01" };
            var server2 = new ServerConfig { Name = "jat-web02" };
            var server3 = new ServerConfig { Name = "jat-web03" };
            var server4 = new ServerConfig { Name = "jat-web04" };
            var server5 = new ServerConfig { Name = "jat-web05" };

            _settingsStopAfterMarkedServer.Config.Servers = new[] { server1, server2, server3, server4, server5 };

            var loadBalancer = new MockLoadBalancer { Mode = LbMode.RoundRobin };

            var sequnceManager = new ExecutionSequenceManager(_settingsStopAfterMarkedServer.Config.Servers, loadBalancer);
            sequnceManager.NewRemoteSequence("Test");

            var status = new StatusReporter();
            sequnceManager.Execute(status, _settingsStopAfterMarkedServer, _token);

            Assert.That(loadBalancer.OnlineOfflineSequence.Count, Is.EqualTo(1));

            Assert.That(loadBalancer.OnlineOfflineSequence[0].Item1, Is.EqualTo("jat-web01"));
            Assert.That(loadBalancer.OnlineOfflineSequence[0].Item2, Is.EqualTo("offline"));
        }

        [Test]
        public void TestThatRoundRobinWithContinueAfterManuelTestOnSpecificServerExecuteCorrectServers()
        {
            var server1 = new ServerConfig { Name = "jat-web01" };
            var server2 = new ServerConfig { Name = "jat-web02" };
            var server3 = new ServerConfig { Name = "jat-web03", StopServer = true };
            var server4 = new ServerConfig { Name = "jat-web04" };
            var server5 = new ServerConfig { Name = "jat-web05" };

            _settingsStopAfterMarkedServer.Config.Servers = new[] { server1, server2, server3, server4, server5 };

            var loadBalancer = new MockLoadBalancer { Mode = LbMode.RoundRobin };

            var sequnceManager = new ExecutionSequenceManager(_settingsStopAfterMarkedServer.Config.Servers, loadBalancer);
            sequnceManager.NewRemoteSequence("Test");

            var status = new StatusReporter();
            sequnceManager.Execute(status, _settingsStopAfterMarkedServer, _token);

            Assert.That(loadBalancer.OnlineOfflineSequence.Count, Is.EqualTo(1));

            Assert.That(loadBalancer.OnlineOfflineSequence[0].Item1, Is.EqualTo("jat-web03"));
            Assert.That(loadBalancer.OnlineOfflineSequence[0].Item2, Is.EqualTo("offline"));
        }

        [Test]
        public void TestThatStickyWithContinueAfterManualTestExecutesOnCorrectServers()
        {
            var server1 = new ServerConfig { Name = "jat-web01" };
            var server2 = new ServerConfig { Name = "jat-web02" };
            var server3 = new ServerConfig { Name = "jat-web03" };
            var server4 = new ServerConfig { Name = "jat-web04" };
            var server5 = new ServerConfig { Name = "jat-web05" };

            _settingsContinueAfterMarkedServer.Config.Servers = new[] { server1, server2, server3, server4, server5 };

            var loadBalancer = new MockLoadBalancer { Mode = LbMode.Sticky };

            var sequnceManager = new ExecutionSequenceManager(_settingsContinueAfterMarkedServer.Config.Servers, loadBalancer);
            sequnceManager.NewRemoteSequence("Test");

            var status = new StatusReporter();
            //remoteSequence.Execute(status, _settingsContinueAfterMarkedServer, _token);
            sequnceManager.Execute(status, _settingsContinueAfterMarkedServer, _token);

            Assert.That(loadBalancer.OnlineOfflineSequence.Count, Is.EqualTo(((_settingsContinueAfterMarkedServer.Config.Servers.Count - 1) * 2) + 1));

            Assert.That(loadBalancer.OnlineOfflineSequence[0].Item1, Is.EqualTo("jat-web01"));
            Assert.That(loadBalancer.OnlineOfflineSequence[0].Item2, Is.EqualTo("online"));

            var serverNumber = 2;
            for (int i = 1; i < loadBalancer.OnlineOfflineSequence.Count; i += 2)
            {
                Assert.That(loadBalancer.OnlineOfflineSequence[i].Item1, Is.EqualTo("jat-web0" + serverNumber));
                Assert.That(loadBalancer.OnlineOfflineSequence[i].Item2, Is.EqualTo("offline"));

                Assert.That(loadBalancer.OnlineOfflineSequence[i + 1].Item1, Is.EqualTo("jat-web0" + serverNumber));
                Assert.That(loadBalancer.OnlineOfflineSequence[i + 1].Item2, Is.EqualTo("online"));
                serverNumber++;
            }
        }

        [Test]
        public void TestThatStickyWithContinueAfterManualTestOnSpecificServerExecutesOnCorrectServers()
        {
            var server1 = new ServerConfig { Name = "jat-web01" };
            var server2 = new ServerConfig { Name = "jat-web02" };
            var server3 = new ServerConfig { Name = "jat-web03", StopServer = true };
            var server4 = new ServerConfig { Name = "jat-web04" };
            var server5 = new ServerConfig { Name = "jat-web05" };

            _settingsContinueAfterMarkedServer.Config.Servers = new[] { server1, server2, server3, server4, server5 };

            var loadBalancer = new MockLoadBalancer { Mode = LbMode.Sticky };

            var sequnceManager = new ExecutionSequenceManager(_settingsContinueAfterMarkedServer.Config.Servers, loadBalancer);
            sequnceManager.NewRemoteSequence("Test");

            var status = new StatusReporter();
            //remoteSequence.Execute(status, _settingsContinueAfterMarkedServer, _token);
            sequnceManager.Execute(status, _settingsContinueAfterMarkedServer, _token);

            Assert.That(loadBalancer.OnlineOfflineSequence.Count, Is.EqualTo(((_settingsContinueAfterMarkedServer.Config.Servers.Count - 1) * 2) + 1));

            Assert.That(loadBalancer.OnlineOfflineSequence[0].Item1, Is.EqualTo("jat-web03"));
            Assert.That(loadBalancer.OnlineOfflineSequence[0].Item2, Is.EqualTo("online"));

            Assert.That(loadBalancer.OnlineOfflineSequence[1].Item1, Is.EqualTo("jat-web01"));
            Assert.That(loadBalancer.OnlineOfflineSequence[1].Item2, Is.EqualTo("offline"));

            Assert.That(loadBalancer.OnlineOfflineSequence[2].Item1, Is.EqualTo("jat-web01"));
            Assert.That(loadBalancer.OnlineOfflineSequence[2].Item2, Is.EqualTo("online"));

            Assert.That(loadBalancer.OnlineOfflineSequence[3].Item1, Is.EqualTo("jat-web02"));
            Assert.That(loadBalancer.OnlineOfflineSequence[3].Item2, Is.EqualTo("offline"));

            Assert.That(loadBalancer.OnlineOfflineSequence[4].Item1, Is.EqualTo("jat-web02"));
            Assert.That(loadBalancer.OnlineOfflineSequence[4].Item2, Is.EqualTo("online"));

            Assert.That(loadBalancer.OnlineOfflineSequence[5].Item1, Is.EqualTo("jat-web04"));
            Assert.That(loadBalancer.OnlineOfflineSequence[5].Item2, Is.EqualTo("offline"));

            Assert.That(loadBalancer.OnlineOfflineSequence[6].Item1, Is.EqualTo("jat-web04"));
            Assert.That(loadBalancer.OnlineOfflineSequence[6].Item2, Is.EqualTo("online"));

            Assert.That(loadBalancer.OnlineOfflineSequence[7].Item1, Is.EqualTo("jat-web05"));
            Assert.That(loadBalancer.OnlineOfflineSequence[7].Item2, Is.EqualTo("offline"));

            Assert.That(loadBalancer.OnlineOfflineSequence[8].Item1, Is.EqualTo("jat-web05"));
            Assert.That(loadBalancer.OnlineOfflineSequence[8].Item2, Is.EqualTo("online"));
        }

        [Test]
        public void TestThatRoundRobinWithContinueAfterManualTestOnSpecificServerExecutesOnCorrectServers()
        {
            var server1 = new ServerConfig { Name = "jat-web01" };
            var server2 = new ServerConfig { Name = "jat-web02" };
            var server3 = new ServerConfig { Name = "jat-web03", StopServer = true };
            var server4 = new ServerConfig { Name = "jat-web04" };
            var server5 = new ServerConfig { Name = "jat-web05" };

            _settingsContinueAfterMarkedServer.Config.Servers = new[] { server1, server2, server3, server4, server5 };

            var loadBalancer = new MockLoadBalancer { Mode = LbMode.RoundRobin };

            var sequnceManager = new ExecutionSequenceManager(_settingsContinueAfterMarkedServer.Config.Servers, loadBalancer);
            sequnceManager.NewRemoteSequence("Test");

            var status = new StatusReporter();
            //remoteSequence.Execute(status, _settingsContinueAfterMarkedServer, _token);
            sequnceManager.Execute(status, _settingsContinueAfterMarkedServer, _token);

            Assert.That(loadBalancer.OnlineOfflineSequence.Count, Is.EqualTo(((_settingsContinueAfterMarkedServer.Config.Servers.Count - 1) * 2) + 1));

            Assert.That(loadBalancer.OnlineOfflineSequence[0].Item1, Is.EqualTo("jat-web01"));
            Assert.That(loadBalancer.OnlineOfflineSequence[0].Item2, Is.EqualTo("offline"));

            Assert.That(loadBalancer.OnlineOfflineSequence[1].Item1, Is.EqualTo("jat-web02"));
            Assert.That(loadBalancer.OnlineOfflineSequence[1].Item2, Is.EqualTo("offline"));

            Assert.That(loadBalancer.OnlineOfflineSequence[2].Item1, Is.EqualTo("jat-web03"));
            Assert.That(loadBalancer.OnlineOfflineSequence[2].Item2, Is.EqualTo("online"));

            Assert.That(loadBalancer.OnlineOfflineSequence[3].Item1, Is.EqualTo("jat-web01"));
            Assert.That(loadBalancer.OnlineOfflineSequence[3].Item2, Is.EqualTo("online"));

            Assert.That(loadBalancer.OnlineOfflineSequence[4].Item1, Is.EqualTo("jat-web02"));
            Assert.That(loadBalancer.OnlineOfflineSequence[4].Item2, Is.EqualTo("online"));

            Assert.That(loadBalancer.OnlineOfflineSequence[5].Item1, Is.EqualTo("jat-web04"));
            Assert.That(loadBalancer.OnlineOfflineSequence[5].Item2, Is.EqualTo("offline"));

            Assert.That(loadBalancer.OnlineOfflineSequence[6].Item1, Is.EqualTo("jat-web05"));
            Assert.That(loadBalancer.OnlineOfflineSequence[6].Item2, Is.EqualTo("offline"));

            Assert.That(loadBalancer.OnlineOfflineSequence[7].Item1, Is.EqualTo("jat-web04"));
            Assert.That(loadBalancer.OnlineOfflineSequence[7].Item2, Is.EqualTo("online"));

            Assert.That(loadBalancer.OnlineOfflineSequence[8].Item1, Is.EqualTo("jat-web05"));
            Assert.That(loadBalancer.OnlineOfflineSequence[8].Item2, Is.EqualTo("online"));
        }
         
    }
}