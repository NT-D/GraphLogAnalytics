using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;
using GenerateTestData.Models;

namespace GenerateTestData.Services
{
    public static class AccessTokenService
    {
        private static HttpClient TokenClient = new HttpClient();

        public static async Task<string> FetchToken()
        {
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
                return responseData.access_token;
            }
            else
            {
                throw new Exception(response.StatusCode.ToString());
            }

        }
    }
}
