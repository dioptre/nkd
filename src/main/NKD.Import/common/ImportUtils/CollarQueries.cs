using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NKD.Import.DataWrappers;
using NKD.Module.BusinessObjects;

namespace NKD.Import.ImportUtils
{
    public class CollarQueries
    {


        internal static List<CollarInfo> FindCollarsForProject(Guid currentSelectedProject)
        {
            List<CollarInfo> ss = new List<CollarInfo>();
            using (var entityObj = new NKDC(BaseImportTools.XSTRING, null))
            {
                IQueryable<Header> res = entityObj.Headers.Where(c => c.ProjectID == currentSelectedProject);
                foreach (Header xx in res)
                {
                    CollarInfo ci = new CollarInfo();
                    ci.Name = xx.HoleName;
                    if (xx.EastingUtm != null)
                    {
                        ci.Easting = (double)xx.EastingUtm;
                    }


                    if (xx.NorthingUtm != null)
                    {
                        ci.Northing = (double)xx.NorthingUtm;
                    }

                    if (xx.Elevation != null)
                    {
                        ci.RL = (double)xx.Elevation;
                    }

                    ss.Add(ci);
                }

                return ss;
            }
        }

        internal static Guid FindHeaderGuid(string headerNameItem, Guid currentSelectedProject)
        {
            Guid resHole = new Guid();
            using (var entityObj = new NKDC(BaseImportTools.XSTRING, null))
            {
                IQueryable<Header> res = entityObj.Headers.Where(c => (c.ProjectID == currentSelectedProject) && (c.HoleName.Equals(headerNameItem)));
                foreach (Header xx in res)
                {
                    resHole = xx.HeaderID;
                }

                return resHole;
            }
        }

        internal static Dictionary<string, Guid> FindHeaderGuidsForProject(Guid NKDProjectID)
        {
            Dictionary<string, Guid> holeIDLookups = new Dictionary<string, Guid>();

            using (var entityObj = new NKDC(BaseImportTools.XSTRING, null))
            {

                IQueryable<Header> res = entityObj.Headers.Where(c => (c.ProjectID == NKDProjectID));
                foreach (Header xx in res)
                {
                    Guid resHole = xx.HeaderID;
                    string ss = xx.HoleName;
                    // only add if it does not exist
                    bool exists = holeIDLookups.ContainsKey(ss);
                    if (!exists)
                    {
                        holeIDLookups.Add(ss, resHole);
                    }
                }

                return holeIDLookups;
            }
        }
    }
}
