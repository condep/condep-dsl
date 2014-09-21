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
            var dependencyHandler = new ArtifactDependencyHandler();
            Assert.That(dependencyHandler.HasDependenciesDefined(new MyArtifactDependentOnStandardArtifact()), Is.True);
        }

        [Test]
        public void TestThat_ArtifactWithoutDependencyIsNotDetected()
        {
            var dependencyHandler = new ArtifactDependencyHandler();
            Assert.That(dependencyHandler.HasDependenciesDefined(new MyStandardArtifact1()), Is.False);
        }

        [Test]
        public void TestThat_ArtifactWithDependencyDetectsCorrectDependency()
        {
            var dependencyHandler = new ArtifactDependencyHandler();
            var settings = new ConDepSettings {Options = {Assembly = GetType().Assembly}};

            var artifact = new MyArtifactDependentOnStandardArtifact();
            dependencyHandler.PopulateWithDependencies(artifact, settings);
            Assert.That(artifact.Dependencies.Single(), Is.InstanceOf<MyStandardArtifact1>());
        }

        [Test]
        public void TestThat_ArtifactWithMultipleDependenciesReturnsCorrectDependenciesInCorrectOrder()
        {
            var dependencyHandler = new ArtifactDependencyHandler();
            var settings = new ConDepSettings { Options = { Assembly = GetType().Assembly } };

            var artifact = new MyArtifactWithMultipleDependencies();
            dependencyHandler.PopulateWithDependencies(artifact, settings);
            var dependencies = artifact.Dependencies.ToList();

            Assert.That(dependencies.Count, Is.EqualTo(2));
            Assert.That(dependencies[0], Is.InstanceOf<MyStandardArtifact1>());
            Assert.That(dependencies[1], Is.InstanceOf<MyStandardArtifact2>());
        }

        [Test]
        public void TestThat_ArtifactWithHierarchicalDependenciesReturnsCorrectDependenciesInCorrectOrder()
        {
            var dependencyHandler = new ArtifactDependencyHandler();
            var settings = new ConDepSettings { Options = { Assembly = GetType().Assembly } };

            var artifact = new MyArtifactWithHierarchicalDependencies();
            dependencyHandler.PopulateWithDependencies(artifact, settings);
            var dependencies = artifact.Dependencies.ToList();

            Assert.That(dependencies.Count, Is.EqualTo(2));
            Assert.That(dependencies[0], Is.InstanceOf<MyStandardArtifact1>());
            Assert.That(dependencies[1], Is.InstanceOf<MyArtifactDependentOnStandardArtifact>());
            
        }
    }

    public class MyStandardArtifact1 : Artifact.Local
    {
        public override void Configure(IOfferLocalOperations onLocalMachine, ConDepSettings settings)
        {

        }
    }

    public class MyStandardArtifact2 : Artifact.Local
    {
        public override void Configure(IOfferLocalOperations onLocalMachine, ConDepSettings settings)
        {

        }
    }

    public class MyArtifactDependentOnStandardArtifact : Artifact.Local, IDependOn<MyStandardArtifact1>
    {
        public override void Configure(IOfferLocalOperations onLocalMachine, ConDepSettings settings)
        {

        }
    }

    public class MyArtifactWithMultipleDependencies : Artifact.Local, IDependOn<MyStandardArtifact1>, IDependOn<MyStandardArtifact2>
    {
        public override void Configure(IOfferLocalOperations onLocalMachine, ConDepSettings settings)
        {

        }
    }

    public class MyArtifactWithHierarchicalDependencies : Artifact.Local, IDependOn<MyArtifactDependentOnStandardArtifact>
    {
        public override void Configure(IOfferLocalOperations onLocalMachine, ConDepSettings settings)
        {
            
        }
    }
}