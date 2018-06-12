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

        private static string token = "eyJ0eXAiOiJKV1QiLCJub25jZSI6IkFRQUJBQUFBQUFEWDhHQ2k2SnM2U0s4MlRzRDJQYjdyWldGVl9rMVpfRmNUZWxpV3h3RGJiNDNWN054UzhzNVN5YjlCS2xjNEJEZy00SDRFQjN0UXZEdVMtYWJETkpDNGRBMkRuWnliUW1ENzdTb2t6d040aVNBQSIsImFsZyI6IlJTMjU2IiwieDV0IjoiaUJqTDFSY3F6aGl5NGZweEl4ZFpxb2hNMllrIiwia2lkIjoiaUJqTDFSY3F6aGl5NGZweEl4ZFpxb2hNMllrIn0.eyJhdWQiOiJodHRwczovL2dyYXBoLm1pY3Jvc29mdC5jb20iLCJpc3MiOiJodHRwczovL3N0cy53aW5kb3dzLm5ldC8zZDE0NWRiMC1mNzViLTRmMjMtYTU1Mi03ODc4OWU3YmE5MmQvIiwiaWF0IjoxNTI4Nzc0NzgwLCJuYmYiOjE1Mjg3NzQ3ODAsImV4cCI6MTUyODc3ODY4MCwiYWlvIjoiWTJkZ1lKajcyblBXaldtVHJ5am5mWkNOMjVaMkVBQT0iLCJhcHBfZGlzcGxheW5hbWUiOiJTa3lwZUdyYXBoQW5hbHl0aWNzIiwiYXBwaWQiOiJiZTg0MmI4NC1kM2E5LTQ4ZjktYjcyZS1lYTUwMDM3NTYzNjkiLCJhcHBpZGFjciI6IjEiLCJpZHAiOiJodHRwczovL3N0cy53aW5kb3dzLm5ldC8zZDE0NWRiMC1mNzViLTRmMjMtYTU1Mi03ODc4OWU3YmE5MmQvIiwib2lkIjoiMDk1ZmUwYzEtZmFhNi00MGYyLWFiMmQtMDkyMzI1ZjdjZjM2Iiwicm9sZXMiOlsiQ2FsZW5kYXJzLlJlYWQiLCJVc2VyLlJlYWQuQWxsIiwiTWFpbC5SZWFkIl0sInN1YiI6IjA5NWZlMGMxLWZhYTYtNDBmMi1hYjJkLTA5MjMyNWY3Y2YzNiIsInRpZCI6IjNkMTQ1ZGIwLWY3NWItNGYyMy1hNTUyLTc4Nzg5ZTdiYTkyZCIsInV0aSI6InZvOFF6Y25IWTBPUlRUdGVjWWNZQUEiLCJ2ZXIiOiIxLjAifQ.cunn23bbUqID_MxrCpVk9x4Q9aFOGCGYIRosZ2bU-4SOrPEyXbwCl8Znj6Qdgc8o38Vh6G1yz4Ej2deeFa9dlEnDjqeju35VRXsQlJAkEZVU5Im3JawxjkwajfNggogvZSwNrqn5idCIP_fbgOMZ9eZ3ApY0Rb6Q0hFLv1WO6CciSrDW6mGYDKq1hsqrpRD99gf_Qz0DezudlXOcxaffLZ8EsTtFKxp4WcekEZ4fbOoaSvGmeD_dGUciwpC6JsywR8Nv3fydjmxBi8w65CqISR3DZ5SG3aUA4Bwr7UtH7k04eMwuaeH_SuT66Q06QDh_evbVXwnUwwZrLfc8DyiY-Q";
        private static string[] ids = new string[] {
            "d25f0cb2-c1a5-48b2-b1a1-0db2ccf37e03",
            "d2db6288-7e35-4911-81c4-11b75973e4fc",
            "b913cdc6-c8eb-4435-acdd-a5a09093feb1",
            "586bf31d-74ce-4382-9668-4c99f9626375",
            "3ad74876-64a0-4707-8e87-361b4c5b97ae",
            "5a9ebe6d-0a82-418e-b433-1a8596f66b4a",
            "d5003c5c-677c-4744-8d5d-b45a949a3c5b"};

        [FunctionName("Function1")]
        public static async Task RunOrchestrator(
            [OrchestrationTrigger] DurableOrchestrationContext context)
        {
            var outputs = new List<string>();

            var provisioningTasks = new List<Task>();
            foreach (var id in ids)
            {
                Task provisionTask = context.CallSubOrchestratorAsync("SubOrchestrator", id);
                provisioningTasks.Add(provisionTask);
            }

            await Task.WhenAll(provisioningTasks);

        }

        [FunctionName("SubOrchestrator")]
        public static async Task SubOrchestrator(
            [OrchestrationTrigger] DurableOrchestrationContext context)
        {
            var userId = context.GetInput<string>();
            var folderId = await context.CallActivityAsync<string>("GetFolderId", new RequestData { UserId = userId });

            var messages = await context.CallActivityAsync<List<ConversationHistoryTableStorage>>("GetMessages", new RequestData { UserId = userId, ConversationHistoryId = folderId });

            foreach (var value in messages)
            {
                await context.CallActivityAsync("InsertIntoStorageTable", value);
            }
        }

        [FunctionName("GetFolderId")]
        public static async Task<string> GetFolderId([ActivityTrigger] RequestData requestData, TraceWriter log)
        {
            httpClient.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);

            var response = await httpClient.GetAsync($"https://graph.microsoft.com/v1.0/users/{requestData.UserId}/mailFolders?$filter=displayName eq 'Conversation History'&$select=id");
            if (response.StatusCode == System.Net.HttpStatusCode.Unauthorized)
            {
                token = await GetNewAccessToken();
            }
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

            httpClient.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);

            var response = await httpClient.GetAsync($"https://graph.microsoft.com/v1.0/users/{requestData.UserId}/mailFolders/{requestData.ConversationHistoryId}/messages?$select=id,body,sender,from,toRecipients");
            if (response.StatusCode == System.Net.HttpStatusCode.Unauthorized)
            {
                token = await GetNewAccessToken();
            }
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

        private static async Task<string> GetNewAccessToken()
        {
            await Task.Delay(5);
            return string.Empty;
        }
    }
}