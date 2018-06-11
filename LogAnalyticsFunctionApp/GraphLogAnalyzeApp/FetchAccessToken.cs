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

            string clientId = "<Your Client ID>";
            string clientSecret = "<Your App Secret>";
            string tokenEndpoint = "https://login.microsoftonline.com/3d145db0-f75b-4f23-a552-78789e7ba92d/oauth2/v2.0/token";

            var requestValues = new List<KeyValuePair<string, string>>();
            requestValues.Add(new KeyValuePair<string, string>("client_id",clientId));
            requestValues.Add(new KeyValuePair<string, string>("scope", "https://graph.microsoft.com/.default"));
            requestValues.Add(new KeyValuePair<string, string>("client_secret",clientSecret));
            requestValues.Add(new KeyValuePair<string, string>("grant_type", "client_credentials"));
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
