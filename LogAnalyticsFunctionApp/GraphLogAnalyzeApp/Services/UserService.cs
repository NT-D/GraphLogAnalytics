using System;
using System.Net.Http;
using System.Threading.Tasks;
using GraphLogAnalyzeApp.Model;

namespace GraphLogAnalyzeApp.Services
{
    public static class UserService
    {
        public static HttpClient usersClient = new HttpClient();

        public static async Task<UserModel> FetchUsers(string token)
        {
            string usersEndpoint = "https://graph.microsoft.com/v1.0/users?$select=id";
            usersClient.DefaultRequestHeaders.Add("Authorization", token);

            var response = await usersClient.GetAsync(usersEndpoint);
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
