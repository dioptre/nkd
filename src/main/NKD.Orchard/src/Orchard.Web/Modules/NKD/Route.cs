using System.Collections.Generic;
using System.Web.Mvc;
using System.Web.Routing;
using Orchard.Mvc.Routes;

namespace NKD
{
    public class Routes : IRouteProvider
    {
        public void GetRoutes(ICollection<RouteDescriptor> routes)
        {
            foreach (var routeDescriptor in GetRoutes())
                routes.Add(routeDescriptor);
        }

        public IEnumerable<RouteDescriptor> GetRoutes()
        {
            return new[] {
                new RouteDescriptor {
                    Priority = 5,
                    Route = new Route(
                        "NKD/{controller}/{action}",
                        new RouteValueDictionary {
                            {"area", "NKD"},
                            {"controller", "User"},
                            {"action", "Index"}
                        },
                        new RouteValueDictionary {
                            {"area", "NKD"},
                            {"controller", "User"}
                        },
                        new RouteValueDictionary {
                            {"area", "NKD"}
                        },
                        new MvcRouteHandler())
                },
                 new RouteDescriptor {
                    Priority = 5,
                    Route = new Route(
                        "NKD/{controller}/{action}/{id}/{verb}",
                        new RouteValueDictionary {
                            {"area", "NKD"},
                            {"controller", "User"}                            
                        },
                        new RouteValueDictionary {
                            {"area", "NKD"},
                            {"controller", "User"},                          
                        },
                        new RouteValueDictionary {
                            {"area", "NKD"}
                        },
                        new MvcRouteHandler())
                }
            };
        }
    }
}