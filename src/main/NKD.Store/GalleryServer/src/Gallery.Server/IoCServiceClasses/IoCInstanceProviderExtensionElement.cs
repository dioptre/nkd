using System;
using System.ServiceModel.Configuration;

namespace Gallery.Server.IoCServiceClasses
{
    public class IoCInstanceProviderExtensionElement : BehaviorExtensionElement
    {
        protected override object CreateBehavior()
        {
            return new IoCServiceBehavior();
        }

        public override Type BehaviorType { get { return typeof (IoCServiceBehavior); } }
    }
}