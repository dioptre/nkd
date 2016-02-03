using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NKD.Module.BusinessObjects;

namespace NKD.Import.ImportUtils
{
    public class ProjectUtils
    {



        internal static Dictionary<Guid, string> GetProjectList()
        {
            Dictionary<Guid, string> projectList = new Dictionary<Guid,string>();
            var result = from p in new NKDC(BaseImportTools.XSTRING, null).Projects select new { p.ProjectName, p.ProjectID };
            foreach (var res in result) { 
                projectList.Add(res.ProjectID, res.ProjectName);                
                
            }
            return projectList;

        }
    }
}
