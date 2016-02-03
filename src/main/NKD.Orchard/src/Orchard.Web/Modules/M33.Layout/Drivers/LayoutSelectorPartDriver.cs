using System;
using M33.Layout.Models;
using M33.Layout.Services;
using Orchard.ContentManagement.Drivers;
using Orchard;
using Orchard.ContentManagement;

namespace M33.Layout.Drivers
{
    public class LayoutSelectorPartDriver : ContentPartDriver<LayoutSelectorPart> {
        private readonly ILayoutSelectorService _layoutService;
        private readonly IWorkContextAccessor _workContextAccessor;

        public LayoutSelectorPartDriver(IWorkContextAccessor workContextAccessor, ILayoutSelectorService layoutService)
        {
  
            _workContextAccessor = workContextAccessor;
            _layoutService = layoutService;
        }

        //DISPLAY
        protected override DriverResult Display(LayoutSelectorPart part, string displayType, dynamic shapeHelper)
        {
            if (!String.IsNullOrWhiteSpace(part.LayoutName) && displayType == "Detail")
            {
                // Get WorkContext
                var wc = _workContextAccessor.GetContext();
                // Add the alternate name to Layout's Metadata. Double underscore '__' translate to a hyphen '-' in the file name.
                wc.Layout.Metadata.Alternates.Add("Layout__"+part.LayoutName);
            }

            // This part doesn't usually display anything
            return null;
        }

        //GET
        protected override DriverResult Editor(LayoutSelectorPart part, dynamic shapeHelper)
        {
            // Make layout names available to drop-down list
            part.AvailableLayouts = _layoutService.GetLayouts();
            return ContentShape("Parts_LayoutSelector_Edit",
                     () => shapeHelper.EditorTemplate(
                         TemplateName: "Parts/LayoutSelector",
                         Model: part,
                //         AvailableLayouts: _layoutService.GetLayouts(),
                         Prefix: Prefix));
        }

        //POST
        protected override DriverResult Editor(LayoutSelectorPart part, IUpdateModel updater, dynamic shapeHelper)
        {
            // This line updates your part from the POST fields.
            updater.TryUpdateModel(part, Prefix, null, null);
            
            // If you need to do any custom processing, you can do so here.

            // Now just display the same editor as before
            return Editor(part, shapeHelper);
        }
    }
}
/*
            if (displayType == "SummaryAdmin")
            {
                // Hide when in Summary Admin (for now)
                return null;
            }
            // Experimental;
            //            var wc = _workContextAccessor.GetContext;
            //          wc.Layout.Metadata.Alternates.Add("Layout_Foo");

*/