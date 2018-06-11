using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Azure.WebJobs.Host;

namespace LogAnalyticsFunctionApp
{
    public static class DurableOrchestrater
    {
        [FunctionName("DurableOrchestrater")]
        public static async Task<List<string>> RunOrchestrator(
            [OrchestrationTrigger] DurableOrchestrationContext context)
        {
            var outputs = new List<string>();

            // Replace "hello" with the name of your Durable Activity Function.
            outputs.Add(await context.CallActivityAsync<string>("DurableOrchestrater_Hello", "Tokyo"));
            outputs.Add(await context.CallActivityAsync<string>("DurableOrchestrater_Hello", "Seattle"));
            outputs.Add(await context.CallActivityAsync<string>("DurableOrchestrater_Hello", "London"));

            // returns ["Hello Tokyo!", "Hello Seattle!", "Hello London!"]
            return outputs;
        }

        [FunctionName("DurableOrchestrater_Hello")]
        public static string SayHello([ActivityTrigger] string name, TraceWriter log)
        {
            log.Info($"Saying hello to {name}.");
            return $"Hello {name}!";
        }

        [FunctionName("DurableOrchestrater_HttpStart")]
        public static async Task<HttpResponseMessage> HttpStart(
            [HttpTrigger(AuthorizationLevel.Anonymous, "get", "post")]HttpRequestMessage req,
            [OrchestrationClient]DurableOrchestrationClient starter,
            TraceWriter log)
        {
            // Function input comes from the request content.
            string instanceId = await starter.StartNewAsync("DurableOrchestrater", null);

            log.Info($"Started orchestration with ID = '{instanceId}'.");

            return starter.CreateCheckStatusResponse(req, instanceId);
        }
    }
}