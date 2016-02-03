using Gallery.DependencyResolution;
using NUnit.Framework;

namespace Gallery.UnitTests.IoC
{
    internal interface IStub {}
    internal class StubImplOne : IStub {}
    internal class StubImplTwo : IStub {}

    public class RegisterTester
    {
        [Test]
        public void ShouldReplaceOldBindingWithGivenBinding()
        {
            Core.IoC.Resolver = new NinjectDependencyResolver();
            Core.IoC.Resolver.Register<IStub, StubImplOne>();
            Core.IoC.Resolver.Register<IStub, StubImplOne>();
            Core.IoC.Resolver.Register<IStub, StubImplTwo>();

            IStub stub = Core.IoC.Resolver.Resolve<IStub>();

            Assert.IsInstanceOf<StubImplTwo>(stub, "Stub should have been instance of StubImplTwo.");
        }
    }
}