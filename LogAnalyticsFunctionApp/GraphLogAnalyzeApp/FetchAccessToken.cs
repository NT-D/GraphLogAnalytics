using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Azure.WebJobs.Host;
using System.Net.Http;
using GraphLogAnalyzeApp.Model;

namespace GraphLogAnalyzeApp
{
    public static class FetchAccessToken
    {
        private static HttpClient TokenClient = new HttpClient();

        [FunctionName("FetchAccessToken")]
        public static async Task<string> FetchToken([ActivityTrigger] string name, TraceWriter log)
        {
            log.Info("FetchAccessToken was called");

            string clientId = Environment.GetEnvironmentVariable("ClientId");
            string clientSecret = Environment.GetEnvironmentVariable("AppSecret");
            string tenantId = Environment.GetEnvironmentVariable("TenantId");
            string tokenEndpoint = $"https://login.microsoftonline.com/{tenantId}/oauth2/v2.0/token";

            var requestValues = new List<KeyValuePair<string, string>>
            {
                new KeyValuePair<string, string>("client_id", clientId),
                new KeyValuePair<string, string>("scope", "https://graph.microsoft.com/.default"),
                new KeyValuePair<string, string>("client_secret", clientSecret),
                new KeyValuePair<string, string>("grant_type", "client_credentials")
            };
            var requestBody = new FormUrlEncodedContent(requestValues);

            var response = await TokenClient.PostAsync(tokenEndpoint, requestBody);
            if (response.IsSuccessStatusCode)
            {
                var responseData = await response.Content.ReadAsAsync<TokenResponse>();
                return $"Access Token is {responseData.access_token}";
            }
            else
            {
                return $"We can't fetch AccessToken";
            }

        }
    }
}
