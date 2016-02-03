using System;

namespace Gallery.Core.Interfaces
{
    public interface IDependencyResolver
    {
        void Register<TBase, TImpl>() where TImpl : TBase;
        void Register<TBase>(Func<TBase> methodToRetrieveImpl);
        TBase Resolve<TBase>();
        object Resolve(Type typeToResolve);
    }
}
