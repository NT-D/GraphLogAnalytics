using System;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using GraphLogAnalyzeApp.Model;

namespace GraphLogAnalyzeApp.Services
{
    public static class UserService
    {
        public static HttpClient usersClient = new HttpClient();
        private static readonly string usersEndpoint = "https://graph.microsoft.com/v1.0/users?$select=id";

        public static async Task<UserModel> FetchUsers(string token)
        {
            //usersClient.DefaultRequestHeaders.Add("Authorization", token);

            var request = new HttpRequestMessage()
            {
                RequestUri = new Uri(usersEndpoint),
                Method = HttpMethod.Get,
            };
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);

            //var response = await usersClient.GetAsync(usersEndpoint);
            var response = await usersClient.SendAsync(request);
            if (response.IsSuccessStatusCode)
            {
                var responseData = await response.Content.ReadAsAsync<UserModel>();
                return responseData;
            }
            else
            {
                throw new Exception(response.StatusCode.ToString());
            }
        }
    }
}
