using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Graph;
using Newtonsoft.Json;

namespace GraphLogAnalyzeApp.Services
{
    public static class UserService
    {
        public static HttpClient usersClient = new HttpClient();

        public static async Task<UserModel> FetchUsers()
        {
            string usersEndpoint = "https://graph.microsoft.com/v1.0/users?$top=5";
            usersClient.DefaultRequestHeaders.Add("Authorization",await AccessTokenService.FetchToken());

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

    public class UserModel
    {
        [JsonProperty("@odata.context")]
        public string odatacontext { get; set; }
        [JsonProperty("@odata.nextLink")]
        public string odatanextLink { get; set; }
        public User[] value { get; set; }
    }
}
