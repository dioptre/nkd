using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.ServiceModel;
using System.ServiceModel.Channels;
using System.ServiceModel.Description;
using System.ServiceModel.Dispatcher;

namespace Gallery.Server.IoCServiceClasses
{
    public class IoCServiceBehavior : IServiceBehavior
    {
        public void Validate(ServiceDescription serviceDescription, ServiceHostBase serviceHostBase)
        { }

        public void AddBindingParameters(ServiceDescription serviceDescription, ServiceHostBase serviceHostBase, Collection<ServiceEndpoint> endpoints,
            BindingParameterCollection bindingParameters)
        { }

        public void ApplyDispatchBehavior(ServiceDescription serviceDescription, ServiceHostBase serviceHostBase)
        {
            List<EndpointDispatcher> endpoints = serviceHostBase.ChannelDispatchers.Cast<ChannelDispatcher>().Where(cd => cd != null)
                .SelectMany(cd => cd.Endpoints).ToList();

            endpoints.ForEach(ep => ep.DispatchRuntime.InstanceProvider = new IoCInstanceProvider(serviceDescription.ServiceType));
        }
    }
}