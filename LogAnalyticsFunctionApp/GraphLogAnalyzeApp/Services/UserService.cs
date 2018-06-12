using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;
using GraphLogAnalyzeApp.Model;

namespace GraphLogAnalyzeApp.Services
{
    public static class UserService
    {
        public static HttpClient usersClient = new HttpClient();

        public static async Task<List<string>> FetchUsers(string token)
        {
            var list = new List<string>();
            string usersEndpoint = "https://graph.microsoft.com/v1.0/users?$select=id";
            usersClient.DefaultRequestHeaders.Add("Authorization", token);
            UserModel responseData;
            do
            {
                var response = await usersClient.GetAsync(usersEndpoint);
                if (response.IsSuccessStatusCode)
                {

                    responseData = await response.Content.ReadAsAsync<UserModel>();
                    foreach (var user in responseData.value)
                    {
                        list.Add(user.id);
                     }
                }
                else
                {
                    throw new Exception(response.StatusCode.ToString());
                }
            } while (string.IsNullOrEmpty(usersEndpoint = responseData.odatanextLink) == false);
            return list;
        }
    }
}
