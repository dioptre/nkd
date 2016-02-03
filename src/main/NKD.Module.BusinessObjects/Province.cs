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

    public class GeospatialValueConverter : ValueConverter
    {

        public override Type StorageType { get { return typeof(byte[]); } }
        public override object ConvertToStorageType(object value)
        {
            throw new NotImplementedException();
        }
        public override object ConvertFromStorageType(object value)
        {
            throw new NotImplementedException();
        }
    }

    public partial class Province 
    {        
        string _coordinates;
        [NotMapped]
        [Size(SizeAttribute.Unlimited)]
        [ValueConverter(typeof(GeospatialValueConverter))]
        [NonPersistent()]
        //[Appearance()]
        public string Coordinates
        {
            get { return (_ProvinceGeography != null) ?  _ProvinceGeography.AsText() : null; }
            set { if (!string.IsNullOrWhiteSpace(value)) ProvinceGeography = DbGeography.FromText(value); else ProvinceGeography = null; }
        }
         
    }
    
    
}
