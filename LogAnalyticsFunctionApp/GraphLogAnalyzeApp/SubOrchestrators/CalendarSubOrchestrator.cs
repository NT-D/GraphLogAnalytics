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
            var events = await context.CallActivityAsync<List<EventsTable>>("GetEventsRelations", new RequestData({ UserId = userId }));
            await context.CallActivityAsync("InsertEventsRelationnToTable", events);
        }

        [FunctionName("GetEventsRelations")]
        public static async Task<List<EventsTable>> GetRelationShipFromEvents([ActivityTrigger] RequestData requestData, TraceWriter log)
        {
            //TODO: Call Calendar service to fetch events with requestData.userId

            var eventsList = new List<EventsTable>();
            //TODO; Change data scheme from Microsoft.Graph.Events to Events Table
            return eventsList;
        }

        [FunctionName("InsertEventsRelationnToTable")]
        public static async Task InsertIntoStorageTable([ActivityTrigger] List<EventsTable> conversationHistoryList, [Table("EventsHistory")]CloudTable cloudTable, TraceWriter log)
        {
            TableBatchOperation tableOperations = new TableBatchOperation();
            foreach (var conversationHistory in conversationHistoryList)
            {
                tableOperations.Insert(conversationHistory);
            }
            await cloudTable.ExecuteBatchAsync(tableOperations);
        }
    }
}
