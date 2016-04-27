using System;
using System.Collections.Generic;
using System.Globalization;
using Microsoft.TeamFoundation.Client;
using Microsoft.TeamFoundation.WorkItemTracking.Client;

namespace ShowMeTheWorkitems
{
    public enum ResponseClosedStatus
    {
        None = 0,
        LooksGood = 1,
        WithComments = 2,
        NeedsWork = 3,
        Declined = 4,
        Removed = 5,
    }

    class Program
    {
        static void Main(string[] args)
        {
            try
            {

                /* User input*/
                string serverName = @"http://{serverName}:8080/tfs/DefaultCollection"; // Server URL needs to have the collection
                string projectName = "{projectName}"; // The project name

                /* Get all the services needed */
                TfsTeamProjectCollection tfs = new TfsTeamProjectCollection(new Uri(serverName));
                WorkItemStore WorkItemStore = tfs.GetService<WorkItemStore>();

                /* Figure out the suggestion query results */
                string queryResponses =
                                        String.Format(CultureInfo.InvariantCulture,
                                        @"SELECT [System.Id], [System.Links.LinkType], [system.WorkItemType], [System.State]
                            FROM WorkItemLinks 
                            WHERE 
                                ([Source].[System.TeamProject] = '{0}'  AND  
                                    [Source].[System.WorkItemType] = '{1}'  AND  
                                    [Source].[System.CreatedBy] = @me)
                                    and
                                ([System.Links.LinkType] = 'System.LinkTypes.Hierarchy-Forward')
                                    and
                                ([Target].[System.WorkItemType] = '{2}'  AND  
                                    [Target].[Microsoft.VSTS.CodeReview.ClosedStatusCode] <> {3}) 
                            ORDER BY [System.ChangedDate] DESC mode(MustContain)",
                                            projectName,
                                            "Code Review Request",
                                            "Code Review Response",
                                            (int)ResponseClosedStatus.Removed);

                Query query = new Query(WorkItemStore, queryResponses);
                List<WorkItemLinkInfo> links = new List<WorkItemLinkInfo>();

                WorkItemLinkInfo[] witLInfo = query.RunLinkQuery();

                Console.WriteLine(witLInfo == null ? "null" : witLInfo.Length.ToString());
            }
            catch (Exception e)
            {
                Console.WriteLine(e.ToString());
            }
        }
    }
}