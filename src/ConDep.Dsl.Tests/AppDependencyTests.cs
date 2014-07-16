using System.Linq;
using ConDep.Dsl.Config;
using ConDep.Dsl.Execution;
using NUnit.Framework;

namespace ConDep.Dsl.Tests
{
    [TestFixture]
    public class AppDependencyTests
    {
        [Test]
        public void TestThat_ArtifactWithDependencyIsDetected()
        {
            var dependencyHandler = new ApplicationDependencyHandler(new MyArtifactDependentOnStandardArtifact());
            Assert.That(dependencyHandler.HasDependenciesDefined(), Is.True);
        }

        [Test]
        public void TestThat_ArtifactWithoutDependencyIsNotDetected()
        {
            var dependencyHandler = new ApplicationDependencyHandler(new MyStandardArtifact1());
            Assert.That(dependencyHandler.HasDependenciesDefined(), Is.False);
        }

        [Test]
        public void TestThat_ArtifactWithDependencyDetectsCorrectDependency()
        {
            var dependencyHandler = new ApplicationDependencyHandler(new MyArtifactDependentOnStandardArtifact());
            var settings = new ConDepSettings {Options = {Assembly = GetType().Assembly}};

            var dependency = dependencyHandler.GetDependeciesForApplication(settings).Single();
            Assert.That(dependency, Is.InstanceOf<MyStandardArtifact1>());
        }

        [Test]
        public void TestThat_ArtifactWithMultipleDependenciesReturnsCorrectDependenciesInCorrectOrder()
        {
            var dependencyHandler = new ApplicationDependencyHandler(new MyArtifactWithMultipleDependencies());
            var settings = new ConDepSettings { Options = { Assembly = GetType().Assembly } };

            var dependencies = dependencyHandler.GetDependeciesForApplication(settings).ToList();
            Assert.That(dependencies.Count, Is.EqualTo(2));
            Assert.That(dependencies[0], Is.InstanceOf<MyStandardArtifact1>());
            Assert.That(dependencies[1], Is.InstanceOf<MyStandardArtifact2>());
        }

        [Test]
        public void TestThat_ArtifactWithHierarchicalDependenciesReturnsCorrectDependenciesInCorrectOrder()
        {
            var dependencyHandler = new ApplicationDependencyHandler(new MyArtifactWithHierarchicalDependencies());
            var settings = new ConDepSettings { Options = { Assembly = GetType().Assembly } };

            var dependencies = dependencyHandler.GetDependeciesForApplication(settings).ToList();
            Assert.That(dependencies.Count, Is.EqualTo(2));
            Assert.That(dependencies[0], Is.InstanceOf<MyStandardArtifact1>());
            Assert.That(dependencies[1], Is.InstanceOf<MyArtifactDependentOnStandardArtifact>());
            
        }
    }

    public class MyStandardArtifact1 : ApplicationArtifact
    {
        public override void Configure(IOfferLocalOperations onLocalMachine, ConDepSettings settings)
        {

        }
    }

    public class MyStandardArtifact2 : ApplicationArtifact
    {
        public override void Configure(IOfferLocalOperations onLocalMachine, ConDepSettings settings)
        {

        }
    }

    public class MyArtifactDependentOnStandardArtifact : ApplicationArtifact, IDependOn<MyStandardArtifact1>
    {
        public override void Configure(IOfferLocalOperations onLocalMachine, ConDepSettings settings)
        {

        }
    }

    public class MyArtifactWithMultipleDependencies : ApplicationArtifact, IDependOn<MyStandardArtifact1>, IDependOn<MyStandardArtifact2>
    {
        public override void Configure(IOfferLocalOperations onLocalMachine, ConDepSettings settings)
        {

        }
    }

    public class MyArtifactWithHierarchicalDependencies : ApplicationArtifact, IDependOn<MyArtifactDependentOnStandardArtifact>
    {
        public override void Configure(IOfferLocalOperations onLocalMachine, ConDepSettings settings)
        {
            
        }
    }
}