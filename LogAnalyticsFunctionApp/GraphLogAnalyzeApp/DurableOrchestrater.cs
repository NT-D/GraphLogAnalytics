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

        private static string token = "eyJ0eXAiOiJKV1QiLCJub25jZSI6IkFRQUJBQUFBQUFEWDhHQ2k2SnM2U0s4MlRzRDJQYjdyUkhqVUxkQkpSVTlPellrV2JnUk8taFJDZUFCZHA2ZFliZ0h4TE9xbkkwbmNFM1RJSjNDdFBXUDhOYjR0Y0NJd3ZZQks1VlROT3NSRDZ6M2tzaUtLNUNBQSIsImFsZyI6IlJTMjU2IiwieDV0IjoiaUJqTDFSY3F6aGl5NGZweEl4ZFpxb2hNMllrIiwia2lkIjoiaUJqTDFSY3F6aGl5NGZweEl4ZFpxb2hNMllrIn0.eyJhdWQiOiJodHRwczovL2dyYXBoLm1pY3Jvc29mdC5jb20iLCJpc3MiOiJodHRwczovL3N0cy53aW5kb3dzLm5ldC8zZDE0NWRiMC1mNzViLTRmMjMtYTU1Mi03ODc4OWU3YmE5MmQvIiwiaWF0IjoxNTI4NzEwMDk2LCJuYmYiOjE1Mjg3MTAwOTYsImV4cCI6MTUyODcxMzk5NiwiYWlvIjoiWTJkZ1lGajluTUh6dm1TaW5kekZWbTNOSmMyekFRPT0iLCJhcHBfZGlzcGxheW5hbWUiOiJTa3lwZUdyYXBoQW5hbHl0aWNzIiwiYXBwaWQiOiJiZTg0MmI4NC1kM2E5LTQ4ZjktYjcyZS1lYTUwMDM3NTYzNjkiLCJhcHBpZGFjciI6IjEiLCJpZHAiOiJodHRwczovL3N0cy53aW5kb3dzLm5ldC8zZDE0NWRiMC1mNzViLTRmMjMtYTU1Mi03ODc4OWU3YmE5MmQvIiwib2lkIjoiMDk1ZmUwYzEtZmFhNi00MGYyLWFiMmQtMDkyMzI1ZjdjZjM2Iiwicm9sZXMiOlsiVXNlci5SZWFkLkFsbCIsIk1haWwuUmVhZCJdLCJzdWIiOiIwOTVmZTBjMS1mYWE2LTQwZjItYWIyZC0wOTIzMjVmN2NmMzYiLCJ0aWQiOiIzZDE0NWRiMC1mNzViLTRmMjMtYTU1Mi03ODc4OWU3YmE5MmQiLCJ1dGkiOiJDX1BLOFhSWmEwZWNLc2d1dzRnVUFBIiwidmVyIjoiMS4wIn0.G5osjzgq32P91GYrpojz0ovSo6podAMygEgVxHZm79D5-VYJXkafp8OTEL4lHiOsODHEcIc-5Cj3NUQ4scgbjaKzY-SV1OZetE6D9i48YmPUHQzKRO1X94GmnLmIRg5eXb5RcFU3JMw2FfnNcW9X5W0ZWUZ2A_vRu8Ufp0GLvQ9t4IacIWlflqwxKnhsaae96VOtxBRd6g75Wi8FHSNAK5cJFg5hHSTKChLqdYTQ7Y5hiN_JRmRZoy7ye85E2dNIHVbP-3KNW5nBattFEZW2unce71SqBEPLVInGRuxmP10K3TsK9YvfkeXECtAyQAuE4N0KDD0N5PuCyK1hdsnvQg";
        private static string[] ids = new string[] {
            "d25f0cb2-c1a5-48b2-b1a1-0db2ccf37e03",
            "d2db6288-7e35-4911-81c4-11b75973e4fc",
            "b913cdc6-c8eb-4435-acdd-a5a09093feb1",
            "586bf31d-74ce-4382-9668-4c99f9626375",
            "3ad74876-64a0-4707-8e87-361b4c5b97ae",
            "5a9ebe6d-0a82-418e-b433-1a8596f66b4a",
            "d5003c5c-677c-4744-8d5d-b45a949a3c5b"};

        [FunctionName("Function1")]
        public static async Task<List<string>> RunOrchestrator(
            [OrchestrationTrigger] DurableOrchestrationContext context)
        {
            var outputs = new List<string>();

            foreach (var id in ids)
            {
                outputs.Add(await context.CallActivityAsync<string>("GetFolderId", new RequestData { UserId = id, AccessToken = token }));
            }

            return outputs;
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