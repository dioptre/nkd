using Orchard.ContentManagement.Records;

namespace M33.Layout.Models
{
    public class LayoutSelectorPartRecord : ContentPartRecord
    {
        public virtual string LayoutName { get; set; }
    }
}