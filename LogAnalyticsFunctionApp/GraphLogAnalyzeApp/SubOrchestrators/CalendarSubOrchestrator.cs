using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Azure.WebJobs.Host;
using Microsoft.WindowsAzure.Storage.Table;
using Newtonsoft.Json;
using GraphLogAnalyzeApp.TableScheme;


namespace GraphLogAnalyzeApp.SubOrchestrators
{
    public static class CalendarSubOrchestrator
    {
        [FunctionName(nameof(CalendarSubOrchestrator))]
        public static async Task SubOrchestrator([OrchestrationTrigger] DurableOrchestrationContext context)
        {
            var userId = context.GetInput<string>();
            var events = await context.CallActivityAsync<List<EventsTable>>("GetEventsRelations", new RequestData() { UserId = userId });
            await context.CallActivityAsync("InsertEventsRelationnToTable", events);
        }

        [FunctionName("GetEventsRelations")]
        public static async Task<List<EventsTable>> GetRelationShipFromEvents([ActivityTrigger] RequestData requestData, TraceWriter log)
        {
            List<Microsoft.Graph.Event> events = await Services.CalendarService.FetchEvents(requestData.UserId, DateTime.Now.AddDays(-6), DateTime.Now.AddDays(1));

            var eventsList = new List<EventsTable>();
            string from;
            foreach (var evt in events)
            {
                from = evt.Organizer.EmailAddress.Name;
                foreach(var attendee in evt.Attendees)
                {
                    eventsList.Add(new EventsTable { From = from, To = attendee.EmailAddress.Name });
                }
            }

            return eventsList;
        }

        [FunctionName("InsertEventsRelationnToTable")]
        public static async Task InsertEventsRelationnToTable([ActivityTrigger] List<EventsTable> relationsFromEvents, [Table("EventsHistory")]CloudTable cloudTable, TraceWriter log)
        {
            TableBatchOperation tableOperations = new TableBatchOperation();
            foreach (var conversationHistory in relationsFromEvents)
            {
                tableOperations.Insert(conversationHistory);
            }
            await cloudTable.ExecuteBatchAsync(tableOperations);
        }
    }
}
