using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Azure.WebJobs.Host;
using Newtonsoft.Json;

namespace GraphLogAnalyzeApp
{

    public static class DurableOrchestrater
    {
        public static HttpClient httpClient = new HttpClient();

        private static string token = "eyJ0eXAiOiJKV1QiLCJub25jZSI6IkFRQUJBQUFBQUFEWDhHQ2k2SnM2U0s4MlRzRDJQYjdyWXlCVU9wWkd0TkswRXlpakpFZ1pVMGVVQmh1VUdSSGVUYVFZalQweE5uNUZNM2ZkWkJRVlZNSnlCUExqVVlJWmRNMml1c2ZERnZEMVhRRmJZTDh0a3lBQSIsImFsZyI6IlJTMjU2IiwieDV0IjoiaUJqTDFSY3F6aGl5NGZweEl4ZFpxb2hNMllrIiwia2lkIjoiaUJqTDFSY3F6aGl5NGZweEl4ZFpxb2hNMllrIn0.eyJhdWQiOiJodHRwczovL2dyYXBoLm1pY3Jvc29mdC5jb20iLCJpc3MiOiJodHRwczovL3N0cy53aW5kb3dzLm5ldC8zZDE0NWRiMC1mNzViLTRmMjMtYTU1Mi03ODc4OWU3YmE5MmQvIiwiaWF0IjoxNTI4NzE0MjkwLCJuYmYiOjE1Mjg3MTQyOTAsImV4cCI6MTUyODcxODE5MCwiYWlvIjoiWTJkZ1lEaXkyY2VtU1BMM2pHY0gvNnpaeThvZkJnQT0iLCJhcHBfZGlzcGxheW5hbWUiOiJTa3lwZUdyYXBoQW5hbHl0aWNzIiwiYXBwaWQiOiJiZTg0MmI4NC1kM2E5LTQ4ZjktYjcyZS1lYTUwMDM3NTYzNjkiLCJhcHBpZGFjciI6IjEiLCJpZHAiOiJodHRwczovL3N0cy53aW5kb3dzLm5ldC8zZDE0NWRiMC1mNzViLTRmMjMtYTU1Mi03ODc4OWU3YmE5MmQvIiwib2lkIjoiMDk1ZmUwYzEtZmFhNi00MGYyLWFiMmQtMDkyMzI1ZjdjZjM2Iiwicm9sZXMiOlsiVXNlci5SZWFkLkFsbCIsIk1haWwuUmVhZCJdLCJzdWIiOiIwOTVmZTBjMS1mYWE2LTQwZjItYWIyZC0wOTIzMjVmN2NmMzYiLCJ0aWQiOiIzZDE0NWRiMC1mNzViLTRmMjMtYTU1Mi03ODc4OWU3YmE5MmQiLCJ1dGkiOiJQS1lRbThrWk9reUpIM3ZYRC04UkFBIiwidmVyIjoiMS4wIn0.ZXZ0eGNN0kW4PjSR-z804StuEjKsOxkMyyTh5Pk6jq9PtBHkBnDr66PPcyE4mVdw-nw1buYr-vE5lWBxMhuc8WV182BKAkL7v8lBN2-x1Z1zBITWQEalylsOfkHTJX0ZhoUrUe0WobF1a6nC6VAoQhAM0vDZd-OpuduwEbPip2rzVV2yWzPRkbZfH5ZZJZf1lp77IsuG2BVeMJ5o6wGtapIo82ExsOH1tVjLu6HDl-NUtNiMCqEfrFSf83XQg8162BOZj9s31Xhiz1fITctUQTKpFOGpcrj4XLP5bDFQTRRhpwMBH7JQovRYyQm6k3TG_uTSQX-eFqBux97EJm_mWw";
        private static string[] ids = new string[] {
            "d25f0cb2-c1a5-48b2-b1a1-0db2ccf37e03",
            "d2db6288-7e35-4911-81c4-11b75973e4fc",
            "b913cdc6-c8eb-4435-acdd-a5a09093feb1",
            "586bf31d-74ce-4382-9668-4c99f9626375",
            "3ad74876-64a0-4707-8e87-361b4c5b97ae",
            "5a9ebe6d-0a82-418e-b433-1a8596f66b4a",
            "d5003c5c-677c-4744-8d5d-b45a949a3c5b"};

        [FunctionName("Function1")]
        public static async Task<List<ResponseData.ValueItem[]>> RunOrchestrator(
            [OrchestrationTrigger] DurableOrchestrationContext context)
        {
            var outputs = new List<string>();

            foreach (var id in ids)
            {
                outputs.Add(await context.CallActivityAsync<string>("GetFolderId", new RequestData { UserId = id, AccessToken = token }));
            }

            var outputs2 = new List<ResponseData.ValueItem[]>();

            for (var i = 0; i < outputs.Count; i++)
            {
                var messages = await context.CallActivityAsync<ResponseData.ValueItem[]>("GetMessages", new RequestData { UserId = ids[i], AccessToken = token, ConversationHistoryId = outputs[i] });
                if(messages.Length > 0)
                {
                    outputs2.Add(messages);
                }

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
        public static async Task<ResponseData.ValueItem[]> GetMessages([ActivityTrigger] RequestData requestData, TraceWriter log)
        {
            
            httpClient.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", requestData.AccessToken);

            var response = await httpClient.GetAsync($"https://graph.microsoft.com/v1.0/users/{requestData.UserId}/mailFolders/{requestData.ConversationHistoryId}/messages?$select=id,body,sender,from,toRecipients");
            response.EnsureSuccessStatusCode();
            var responseData = JsonConvert.DeserializeObject<ResponseData>(await response.Content.ReadAsStringAsync());

            return responseData.Value;
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
    }
}