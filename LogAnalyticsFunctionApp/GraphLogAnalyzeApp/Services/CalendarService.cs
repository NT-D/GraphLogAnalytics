using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Net.Http;
using GraphLogAnalyzeApp.Model;

namespace GraphLogAnalyzeApp.Services
{
    public class CalendarService
    {
        private static HttpClient Client = new HttpClient();

        public static async Task<List<Microsoft.Graph.Event>> FetchEvents(string userId, DateTime startDateTime, DateTime endDateTime)
        {
            /* for test
            int pageSize = 2;
            string requestUri = $"https://graph.microsoft.com/v1.0/users/{userId}/calendarView?startDateTime={startDateTime:yyyy-MM-dd}&endDateTime={endDateTime:yyyy-MM-dd}&$top={pageSize}";
            */
            string requestUri = $"https://graph.microsoft.com/v1.0/users/{userId}/calendarView?startDateTime={startDateTime:yyyy-MM-dd}&endDateTime={endDateTime:yyyy-MM-dd}&$select=Organizer,Attendees";
            string accessToken = await AccessTokenService.FetchToken();
            Client.DefaultRequestHeaders.Add("Authorization", $"Bearer {accessToken}");

            HttpResponseMessage response;
            EventModel responseData;
            var events = new List<Microsoft.Graph.Event>();
            while (requestUri != null)
            {
                response = await Client.GetAsync(requestUri);
                if (response.IsSuccessStatusCode)
                {
                    responseData = await response.Content.ReadAsAsync<EventModel>();
                    events.AddRange(responseData.value);
                    requestUri = responseData.odatanextlink;
                }
                else
                {
                    throw new Exception(response.StatusCode.ToString());
                }
            }

            return events;
        }

    }
}
