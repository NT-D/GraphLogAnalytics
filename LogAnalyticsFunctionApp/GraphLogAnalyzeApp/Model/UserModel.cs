using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace GraphLogAnalyzeApp.Model
{
    public class UserModel
    {
        [JsonProperty("@odata.context")]
        public string odatacontext { get; set; }
        [JsonProperty("@odata.nextLink")]
        public string odatanextLink { get; set; }
        public User[] value { get; set; }
    }

    public class User
    {
        public string id { get; set; }
    }

}
