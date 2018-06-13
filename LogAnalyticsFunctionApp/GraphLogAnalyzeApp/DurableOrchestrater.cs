using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;
using GraphLogAnalyszeApp;
using GraphLogAnalyzeApp.Model;
using GraphLogAnalyzeApp.Services;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Azure.WebJobs.Host;
using Microsoft.WindowsAzure.Storage.Table;
using Newtonsoft.Json;
using GraphLogAnalyzeApp.SubOrchestrators;

namespace GraphLogAnalyzeApp
{

    public static class DurableOrchestrater
    {
        public static HttpClient httpClient = new HttpClient();

        private static string token = "<Your token>";

        [FunctionName("Function1")]
        public static async Task RunOrchestrator(
            [OrchestrationTrigger] DurableOrchestrationContext context)
        {
            var ids = await context.CallActivityAsync<List<string>>("GetUsers", null);

            var provisioningTasks = new List<Task>();
            foreach (var id in ids)
            {
                //For Skype
                Task provisionTask = context.CallSubOrchestratorAsync("SubOrchestrator", id);
                provisioningTasks.Add(provisionTask);

                //For Calendar
                Task eventsTask = context.CallSubOrchestratorAsync("CalendarSubOrchestrator", id);
                provisioningTasks.Add(eventsTask);
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
            if (messages.Count > 0)
            {
                await context.CallActivityAsync("InsertIntoStorageTable", messages);
            }
        }

        [FunctionName("GetUsers")]
        public static async Task<List<String>> GetUsers([ActivityTrigger] DurableActivityContext context, TraceWriter log)
        {
            var token = await AccessTokenService.FetchToken();
            var ids = await UserService.FetchUsers(token);
            return ids;
        }

        [FunctionName("GetFolderId")]
        public static async Task<string> GetFolderId([ActivityTrigger] RequestData requestData, TraceWriter log)
        {
            httpClient.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);

            var response = await httpClient.GetAsync($"https://graph.microsoft.com/v1.0/users/{requestData.UserId}/mailFolders?$filter=displayName eq 'Conversation History'&$select=id");
            if (response.StatusCode == System.Net.HttpStatusCode.Unauthorized)
            {
                token = await AccessTokenService.FetchToken();
                httpClient.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);
                response = await httpClient.GetAsync($"https://graph.microsoft.com/v1.0/users/{requestData.UserId}/mailFolders?$filter=displayName eq 'Conversation History'&$select=id");
            }
            response.EnsureSuccessStatusCode();
            var responseData = JsonConvert.DeserializeObject<ResponseData>(await response.Content.ReadAsStringAsync());

            if (responseData.Value.Length == 1)
            {
                return responseData.Value[0].id;
            }
            else
            {
                response = await httpClient.GetAsync($"https://graph.microsoft.com/v1.0/users/{requestData.UserId}/mailFolders?$filter=displayName eq '会話の履歴'&$select=id");
                response.EnsureSuccessStatusCode();
                responseData = JsonConvert.DeserializeObject<ResponseData>(await response.Content.ReadAsStringAsync());
                return responseData.Value[0].id;
            }
        }

        [FunctionName("GetMessages")]
        public static async Task<List<ConversationHistoryTableStorage>> GetMessages([ActivityTrigger] RequestData requestData, TraceWriter log)
        {
            var list = new List<ConversationHistoryTableStorage>();
            string endpoint = $"https://graph.microsoft.com/v1.0/users/{requestData.UserId}/mailFolders/{requestData.ConversationHistoryId}/messages?$select=id,body,sender,from,toRecipients";

            ResponseData responseData;
            do
            {

                httpClient.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);

                var response = await httpClient.GetAsync(endpoint);
                if (response.StatusCode == System.Net.HttpStatusCode.Unauthorized)
                {
                    token = await AccessTokenService.FetchToken();
                    httpClient.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);
                    response = await httpClient.GetAsync(endpoint);
                }
                response.EnsureSuccessStatusCode();
                responseData = JsonConvert.DeserializeObject<ResponseData>(await response.Content.ReadAsStringAsync());

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
            } while (String.IsNullOrEmpty(endpoint = responseData.odatanextLink) == false);
            return list;
        }

        [FunctionName("InsertIntoStorageTable")]
        public static async Task InsertIntoStorageTable([ActivityTrigger] List<ConversationHistoryTableStorage> conversationHistoryList, [Table("ConversationHistory")]CloudTable cloudTable, TraceWriter log)
        {
            TableBatchOperation tableOperations = new TableBatchOperation();
            foreach (var conversationHistory in conversationHistoryList)
            {
                tableOperations.Insert(conversationHistory);
            }
            await cloudTable.ExecuteBatchAsync(tableOperations);
        }

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