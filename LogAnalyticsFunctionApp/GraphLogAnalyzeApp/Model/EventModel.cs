using Newtonsoft.Json;

namespace GraphLogAnalyzeApp.Model
{
    public class EventModel
    {
        [JsonProperty("@odata.context")]
        public string odatacontext { get; set; }

        [JsonProperty("@odata.nextLink")]
        public string odatanextlink { get; set; }

        public Microsoft.Graph.Event[] value { get; set; }
    }

}
