using System.Collections.Generic;
using System.Linq;

namespace M33.Layout.Services
{
    public class LayoutSelectorService : ILayoutSelectorService
    {

        private readonly IEnumerable<ILayoutAlternatesProvider> _providers;

        public LayoutSelectorService(IEnumerable<ILayoutAlternatesProvider> providers)
        {
            _providers = providers;
        }
        
        public IEnumerable<string> GetLayouts() {
            return _providers.SelectMany(p => p.GetLayouts());
        }
    }
}