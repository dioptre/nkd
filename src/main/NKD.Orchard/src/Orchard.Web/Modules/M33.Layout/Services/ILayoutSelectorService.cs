using System.Collections.Generic;
using Orchard;

namespace M33.Layout.Services
{
    public interface ILayoutSelectorService : IDependency
    {
        IEnumerable<string> GetLayouts();
    }
}