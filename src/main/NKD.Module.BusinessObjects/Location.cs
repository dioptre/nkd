using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DevExpress.Persistent.Base;
using System.IO;
using DevExpress.Xpo;
using DevExpress.Xpo.Metadata;
using System.Data.Spatial;
using System.ComponentModel.DataAnnotations.Schema;

namespace NKD.Module.BusinessObjects
{

    public partial class Location 
    {        
        string _coordinates;
        [NotMapped]
        [Size(SizeAttribute.Unlimited)]
        [ValueConverter(typeof(GeospatialValueConverter))]
        [NonPersistent()]
        //[Appearance()]
        public string Coordinates
        {
            get { return (_LocationGeography != null) ? _LocationGeography.AsText() : null; }
            set { if (!string.IsNullOrWhiteSpace(value)) LocationGeography = DbGeography.FromText(value); else LocationGeography = null; }
        }
         
    }
    
    
}
