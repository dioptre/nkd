using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.Serialization;
using NKD.Import.FormatSpecification;

namespace NKD.Import.FormatSpecificationIO
{
    public class ImportMapIO
    {

        public static void SaveImportMap(ImportDataMap importDataMap, string filename)
        {

            // check the map is not null
            if (importDataMap != null && filename != null)
            {
                Type importDatMapType = importDataMap.GetType();

                var serializer = new XmlSerializer(importDatMapType);
                using (var definitionWriter = XmlWriter.Create(filename))
                {
                    serializer.Serialize(definitionWriter, importDataMap);
                }

            }

        }


        public static ImportDataMap LoadImportMap(string filename)
        {
            ImportDataMap impMap = null;
            if (filename != null)
            {
                var serializer = new XmlSerializer(typeof(ImportDataMap));
                using (var reader = XmlReader.Create(filename))
                {
                    impMap = (ImportDataMap)serializer.Deserialize(reader);
                }
            }
            return impMap;

        }

        public static ImportDataMap LoadImportMap(Stream fileAsStream)
        {
            ImportDataMap impMap = null;
            if (fileAsStream != null)
            {
                var serializer = new XmlSerializer(typeof(ImportDataMap));
                using (var reader = XmlReader.Create(fileAsStream))
                {
                    impMap = (ImportDataMap)serializer.Deserialize(reader);
                }
            }
            return impMap;

        }

    }
}
