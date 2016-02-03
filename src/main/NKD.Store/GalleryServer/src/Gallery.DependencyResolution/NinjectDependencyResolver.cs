using System;
using Gallery.Core.Exceptions;
using Gallery.Core.Interfaces;
using Ninject;
using Ninject.Extensions.Logging.Log4net;

namespace Gallery.DependencyResolution
{
    public class NinjectDependencyResolver : IDependencyResolver
    {
        private readonly IKernel _kernel = new StandardKernel(new NinjectSettings { LoadExtensions = false }, new Log4NetModule());

        public void Register<TBase, TImpl>() where TImpl : TBase
        {
            _kernel.Unbind<TBase>();
            _kernel.Bind<TBase>().To<TImpl>();
        }

        public void Register<TBase>(Func<TBase> methodToRetrieveImpl)
        {
            _kernel.Unbind<TBase>();
            _kernel.Bind<TBase>().ToMethod(c => methodToRetrieveImpl());
        }

        public TBase Resolve<TBase>()
        {
            try
            {
                return _kernel.Get<TBase>();
            }
            catch (ActivationException activationException)
            {
                throw new IoCResolutionFailedException(typeof(TBase), activationException);
            }
        }

        public object Resolve(Type typeToResolve)
        {
            try
            {
                return _kernel.Get(typeToResolve);
            }
            catch (ActivationException activationException)
            {
                throw new IoCResolutionFailedException(typeToResolve, activationException);
            }
        }
    }
}