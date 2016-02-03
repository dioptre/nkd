using System.Collections.Generic;

namespace M33.Layout.Services
{
    public class DefaultLayoutAlternatesProvider : ILayoutAlternatesProvider
    {
        public IEnumerable<string> GetLayouts()
        {
            yield return "Default";
        }
    }
}