using System.Collections.Generic;
using Orchard;

namespace M33.Layout.Services
{
    public interface ILayoutAlternatesProvider : IDependency
    {
        IEnumerable<string> GetLayouts();
    }
}