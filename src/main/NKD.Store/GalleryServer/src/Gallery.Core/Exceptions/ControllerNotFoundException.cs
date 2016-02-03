using System;

namespace Gallery.Core.Exceptions
{
    public class ControllerNotFoundException : Exception
    {
        public ControllerNotFoundException(string controllerName, string action, Exception innerException)
            :base(string.Format("Action controller 'Controllers.{0}.{1}Controller' could not be found. " +
                "Either it or one of its dependencies are not registered.", controllerName, action), innerException)
        {}
    }
}