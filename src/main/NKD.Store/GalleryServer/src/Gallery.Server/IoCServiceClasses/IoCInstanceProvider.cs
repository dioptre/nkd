using System;
using System.ServiceModel;
using System.ServiceModel.Channels;
using System.ServiceModel.Dispatcher;
using Gallery.Core;

namespace Gallery.Server.IoCServiceClasses
{
    public class IoCInstanceProvider : IInstanceProvider
    {
        private readonly Type _serviceType;

        public IoCInstanceProvider(Type serviceType)
        {
            _serviceType = serviceType;
        }

        public object GetInstance(InstanceContext instanceContext)
        {
            return GetInstance(instanceContext, null);
        }

        public object GetInstance(InstanceContext instanceContext, Message message)
        {
            return IoC.Resolver.Resolve(_serviceType);
        }

        public void ReleaseInstance(InstanceContext instanceContext, object instance)
        { }
    }
}