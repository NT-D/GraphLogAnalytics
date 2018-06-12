using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;
using GraphLogAnalyszeApp;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Azure.WebJobs.Host;
using Microsoft.WindowsAzure.Storage.Table;
using Newtonsoft.Json;

namespace GraphLogAnalyzeApp
{

    public static class DurableOrchestrater
    {
        public static HttpClient httpClient = new HttpClient();

        private static string token = "eyJ0eXAiOiJKV1QiLCJub25jZSI6IkFRQUJBQUFBQUFEWDhHQ2k2SnM2U0s4MlRzRDJQYjdyOE5HMDlvSWw0aFg3XzF2RDJXV3g4S25kRlpiNlRnb0hUZG1xM2ozbTAyWDVoVG5fVE1zbVp0a2FjRHVGTXhTMG80LVdmTi1rd3FBbFZ5RmlLMEJPSnlBQSIsImFsZyI6IlJTMjU2IiwieDV0IjoiaUJqTDFSY3F6aGl5NGZweEl4ZFpxb2hNMllrIiwia2lkIjoiaUJqTDFSY3F6aGl5NGZweEl4ZFpxb2hNMllrIn0.eyJhdWQiOiJodHRwczovL2dyYXBoLm1pY3Jvc29mdC5jb20iLCJpc3MiOiJodHRwczovL3N0cy53aW5kb3dzLm5ldC8zZDE0NWRiMC1mNzViLTRmMjMtYTU1Mi03ODc4OWU3YmE5MmQvIiwiaWF0IjoxNTI4NzcwNTExLCJuYmYiOjE1Mjg3NzA1MTEsImV4cCI6MTUyODc3NDQxMSwiYWlvIjoiWTJkZ1lHQzc3WHZEUnlROXFXSFcrWlcvOXF4OEJnQT0iLCJhcHBfZGlzcGxheW5hbWUiOiJTa3lwZUdyYXBoQW5hbHl0aWNzIiwiYXBwaWQiOiJiZTg0MmI4NC1kM2E5LTQ4ZjktYjcyZS1lYTUwMDM3NTYzNjkiLCJhcHBpZGFjciI6IjEiLCJpZHAiOiJodHRwczovL3N0cy53aW5kb3dzLm5ldC8zZDE0NWRiMC1mNzViLTRmMjMtYTU1Mi03ODc4OWU3YmE5MmQvIiwib2lkIjoiMDk1ZmUwYzEtZmFhNi00MGYyLWFiMmQtMDkyMzI1ZjdjZjM2Iiwicm9sZXMiOlsiVXNlci5SZWFkLkFsbCIsIk1haWwuUmVhZCJdLCJzdWIiOiIwOTVmZTBjMS1mYWE2LTQwZjItYWIyZC0wOTIzMjVmN2NmMzYiLCJ0aWQiOiIzZDE0NWRiMC1mNzViLTRmMjMtYTU1Mi03ODc4OWU3YmE5MmQiLCJ1dGkiOiJ1cjZycnhFU0dVdXQxV0xDSDBnWkFBIiwidmVyIjoiMS4wIn0.NuxxknVCfEIr3SZ9yHuDlv3vS8sczZnGjJdhi9CD6XfunZQ9BmxEgPGPg3y-UWn1YFG0tS8dW2m9v6yzYRioF0d6fkH3D3z5-tLSk18f0701Ct7WkrmY6QnBE5S6QlmG_RXzdqv6XomQFmMjpIr66WWnLYabln2CnZzi-6x6N0Hv_-jn_4yDkYTQI815g2JfxjuHXkU6xNyJvjTlaY1eQw-hXdA_y6BqOljF1mfYsgQWTpGYtw6-Cl_JiLmNTxFZTAl-SQlNwc7jGDj5iXuvwD9_Huw7JIH5CYBLBiuCJn2kjuUC7HIlMwm_IhyFPLi9yBc9rp6laNSWfe_jXOyywg";
        private static string[] ids = new string[] {
            "d25f0cb2-c1a5-48b2-b1a1-0db2ccf37e03",
            "d2db6288-7e35-4911-81c4-11b75973e4fc",
            "b913cdc6-c8eb-4435-acdd-a5a09093feb1",
            "586bf31d-74ce-4382-9668-4c99f9626375",
            "3ad74876-64a0-4707-8e87-361b4c5b97ae",
            "5a9ebe6d-0a82-418e-b433-1a8596f66b4a",
            "d5003c5c-677c-4744-8d5d-b45a949a3c5b"};

        [FunctionName("Function1")]
        public static async Task<List<ConversationHistoryTableStorage>> RunOrchestrator(
            [OrchestrationTrigger] DurableOrchestrationContext context)
        {
            var outputs = new List<string>();

            foreach (var id in ids)
            {
                outputs.Add(await context.CallActivityAsync<string>("GetFolderId", new RequestData { UserId = id, AccessToken = token }));
            }

            var outputs2 = new List<ConversationHistoryTableStorage>();

            for (var i = 0; i < outputs.Count; i++)
            {
                var messages = await context.CallActivityAsync<List<ConversationHistoryTableStorage>>("GetMessages", new RequestData { UserId = ids[i], AccessToken = token, ConversationHistoryId = outputs[i] });
                if(messages.Count > 0)
                {
                    outputs2.AddRange(messages);
                }

            }

            foreach (var value in outputs2)
            {
                await context.CallActivityAsync("InsertIntoStorageTable", value);
            }

            return outputs2;
        }

        [FunctionName("GetFolderId")]
        public static async Task<string> GetFolderId([ActivityTrigger] RequestData requestData, TraceWriter log)
        {
            httpClient.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", requestData.AccessToken);
            
            var response = await httpClient.GetAsync($"https://graph.microsoft.com/v1.0/users/{requestData.UserId}/mailFolders?$filter=displayName eq 'Conversation History'&$select=id");
            response.EnsureSuccessStatusCode();
            var responseData = JsonConvert.DeserializeObject<ResponseData>(await response.Content.ReadAsStringAsync());

            if (responseData.Value.Length == 1)
            {
                return responseData.Value[0].id;
            } else
            {
                response = await httpClient.GetAsync($"https://graph.microsoft.com/v1.0/users/{requestData.UserId}/mailFolders?$filter=displayName eq '‰ï˜b‚Ì—š—ð'&$select=id");
                response.EnsureSuccessStatusCode();
                responseData = JsonConvert.DeserializeObject<ResponseData>(await response.Content.ReadAsStringAsync());
                return responseData.Value[0].id;
            }
        }

        [FunctionName("GetMessages")]
        public static async Task<List<ConversationHistoryTableStorage>> GetMessages([ActivityTrigger] RequestData requestData, TraceWriter log)
        {
            
            httpClient.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", requestData.AccessToken);

            var response = await httpClient.GetAsync($"https://graph.microsoft.com/v1.0/users/{requestData.UserId}/mailFolders/{requestData.ConversationHistoryId}/messages?$select=id,body,sender,from,toRecipients");
            response.EnsureSuccessStatusCode();
            var responseData = JsonConvert.DeserializeObject<ResponseData>(await response.Content.ReadAsStringAsync());

            var list = new List<ConversationHistoryTableStorage>();

            foreach (var data in responseData.Value)
            {
                foreach (var to in data.toRecipients)
                {
                    var fromAddress = data.from.emailAddress.name;
                    var toAddress = to.emailAddress.name;
                    if (fromAddress.Equals(toAddress) == false)
                    {
                        var domain = data.from.emailAddress.address.Split('@')[1];
                        list.Add(new ConversationHistoryTableStorage { PartitionKey = domain, RowKey = Guid.NewGuid().ToString(), From = fromAddress, To = toAddress });
                    }
                }
            }
            return list;
        }

        [FunctionName("InsertIntoStorageTable")]
        [return: Table("ConversationHistory")]
        public static ConversationHistoryTableStorage InsertIntoStorageTable([ActivityTrigger] ConversationHistoryTableStorage conversationHistory, TraceWriter log)
        {
            return conversationHistory;
        }

        /*
        [FunctionName("InsertIntoStorageTable")]
        [return: Table("ConversationHistory")]
        public static async Task InsertIntoStorageTable([ActivityTrigger] ResponseData.ValueItem[] responseData, CloudTable cloudTable, TraceWriter log)
        {
            TableBatchOperation tableOperations = new TableBatchOperation();
            foreach (var data in responseData)
            {
                foreach (var to in data.toRecipients)
                {
                    var fromAddress = data.from.emailAddress.address;
                    var toAddress = to.emailAddress.address;
                    if (fromAddress.Equals(toAddress) == false)
                    {
                        var domain = data.from.emailAddress.address.Split('@')[1];
                        tableOperations.Insert(new ConversationHistoryTableStorage { PartitionKey = domain, RowKey = Guid.NewGuid().ToString(), From = fromAddress, To = toAddress });
                    }
                }
            }
            await cloudTable.ExecuteBatchAsync(tableOperations);
        }
        */

        [FunctionName("Function1_HttpStart")]
        public static async Task<HttpResponseMessage> HttpStart(
            [HttpTrigger(AuthorizationLevel.Anonymous, "get", "post")]HttpRequestMessage req,
            [OrchestrationClient]DurableOrchestrationClient starter,
            TraceWriter log)
        {
            // Function input comes from the request content.
            string instanceId = await starter.StartNewAsync("Function1", null);

            log.Info($"Started orchestration with ID = '{instanceId}'.");

            return starter.CreateCheckStatusResponse(req, instanceId);
        }
    }
}