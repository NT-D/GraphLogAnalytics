using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using System.Net;
using GenerateTestData.Models;

namespace GenerateTestData.Services
{
    public static class UsersService
    {
        private static readonly string usersEndpoint = "https://graph.microsoft.com/v1.0/users";
        private static HttpClient userClient = new HttpClient();
        
        public async static Task<UserModel> fetchAllUsers(string token)
        {
            userClient.DefaultRequestHeaders.Clear();
            userClient.DefaultRequestHeaders.Add("Authorization", $"Bearer {token}");

            var response = await userClient.GetAsync(usersEndpoint);
            if (response.IsSuccessStatusCode)
            {
                var responseData = await response.Content.ReadAsAsync<UserModel>();
                Console.WriteLine("Fetched data");
                return responseData;
            }
            if(response.StatusCode.ToString() == "429")
            {
                var header = response.Headers;
                throw new Exception("Too many requests");
            }
            else
            {
                throw new Exception(response.StatusCode.ToString());
            }
        }
    }
}
