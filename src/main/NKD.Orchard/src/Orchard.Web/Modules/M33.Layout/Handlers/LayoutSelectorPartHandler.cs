using M33.Layout.Models;
using Orchard.ContentManagement.Handlers;
using Orchard.Data;

namespace M33.Layout.Handlers
{
    public class LayoutSelectorPartHandler : ContentHandler
    {
        public LayoutSelectorPartHandler(IRepository<LayoutSelectorPartRecord> repository)
        {
            Filters.Add(StorageFilter.For(repository));
        }
    }
}