using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using System.Activities;
using System.Reflection;
using System.Collections.ObjectModel;
using System.Activities.XamlIntegration;
using System.IO;
using System.ServiceModel.Activities.Description;

namespace NKD.Workflow.Helpers
{
    public class XAMLHelper
    {
            private string xaml;

            public XAMLHelper(string xaml)
            {
                this.xaml = xaml;

                DynamicActivity act = GetRuntimeExecutionRoot(this.xaml);
                ArgumentNames = GetArgumentsNames(act);

                GetVariables(act);
            }

            private void GetVariables(DynamicActivity act)
            {
                Variables = new List<string>();
                InspectActivity(act);


            }

            private void InspectActivity(Activity root)
            {


                IEnumerator<Activity> activities =
                    WorkflowInspectionServices.GetActivities(root).GetEnumerator();


                while (activities.MoveNext())
                {

                    PropertyInfo propVars = activities.Current.GetType().GetProperties().FirstOrDefault(p => p.Name == "Variables" && p.PropertyType == typeof(Collection<Variable>));
                    if (propVars != null)
                    {
                        try
                        {
                            Collection<Variable> variables = (Collection<Variable>)propVars.GetValue(activities.Current, null);
                            variables.ToList().ForEach(v =>
                            {
                                Variables.Add(v.Name);

                            });
                        }
                        catch
                        {

                        }
                    }
                    InspectActivity(activities.Current);
                }
            }


            public List<string> Variables
            {
                get;
                private set;
            }

            public List<string> ArgumentNames
            {
                get;
                private set;
            }


            private DynamicActivity GetRuntimeExecutionRoot(string xaml)
            {


                Activity root = ActivityXamlServices.Load(new StringReader(xaml));


                WorkflowInspectionServices.CacheMetadata(root);

                return root as DynamicActivity;

            }

            private List<string> GetArgumentsNames(DynamicActivity act)
            {
                List<string> names = new List<string>();
                if (act != null)
                {
                    act.Properties.Where(p => typeof(Argument).IsAssignableFrom(p.Type)).ToList().ForEach(dp =>
                    {
                        names.Add(dp.Name);
                    });

                }

                return names;
            }

          
        
    }
}
