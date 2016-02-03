using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Activities.Tracking;
using System.Activities.Runtime;

namespace NKD.Workflow.Helpers
{
    public class TrackingHelper
    {
        private static TrackingQuery GetActivityQueryState(List<string> argumentNames, List<string> variablesNames)
        {
            ActivityStateQuery query = new ActivityStateQuery()
            {
                ActivityName = "*",
                States = { ActivityStates.Executing, ActivityStates.Closed }
            };
            if (argumentNames != null)
            {
                argumentNames.Distinct().ToList().ForEach(arg =>
                {
                    query.Arguments.Add(arg);
                });
            }
            if (variablesNames != null)
            {
                variablesNames.Distinct().ToList().ForEach(v =>
                {
                    query.Variables.Add(v);
                });
            }
            return query;
        }

        private static TrackingProfile CreateTrace(List<string> argumentNames, List<string> variableNames)
        {
            var trackingProfile = new TrackingProfile()
            {
                ImplementationVisibility = ImplementationVisibility.All,
                Name = "CustomTrackingProfile",
                Queries = 
                {
                    new CustomTrackingQuery() 
                    {
                     Name = "*",
                     ActivityName = "*"
                    },
                    new WorkflowInstanceQuery()
                    {


                         // Limit workflow instance tracking records for started and completed workflow states
                        States = {WorkflowInstanceStates.Started, WorkflowInstanceStates.Completed },
                   }
                }
            };

            trackingProfile.Queries.Add(GetActivityQueryState(argumentNames, variableNames));

            return trackingProfile;
        }

        public static TrackingProfile SimpleProfile = new TrackingProfile
                {
                    Name = "SimpleProfile",
                    Queries = {
                            new WorkflowInstanceQuery {
                                States = { "*" }
                            },
                            new ActivityStateQuery {
                                States={ "*" }
                            },
                            new CustomTrackingQuery {
                                ActivityName = "*",
                                Name = "*"
                            }
                        }
                };

        public class DebugTrackingParticipant : TrackingParticipant // Use EtwTrackingParticipant for event data
        {
            protected override void Track(TrackingRecord record, TimeSpan timeout)
            {
                if (System.Diagnostics.Debugger.IsAttached)
                {
                    ActivityStateRecord aRecord = record as ActivityStateRecord;
                    if (aRecord != null)
                    {
                        System.Diagnostics.Debug.WriteLine("{0} entered state {1}", aRecord.Activity.Name, aRecord.State);
                        foreach (var item in aRecord.Arguments)
                        {
                            System.Diagnostics.Debug.WriteLine("Argument:{0} has value: {1}", item.Key, item.Value);
                        }
                        foreach (var item in aRecord.Variables)
                        {
                            System.Diagnostics.Debug.WriteLine("Variable:{0} has value: {1}", item.Key, item.Value);
                        }
                    }
                }
            }
         
        }
    }
}
