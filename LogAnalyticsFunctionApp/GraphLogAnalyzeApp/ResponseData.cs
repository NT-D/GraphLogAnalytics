using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GraphLogAnalyzeApp
{
    public class ResponseData
    {

        public ValueItem[] Value { get; set; }

        public class ValueItem
        {
            public string id { get; set; }
            public ToRecipients[] toRecipients { get; set; }
            public ToRecipients from { get; set; }

            public class ToRecipients
            {
                public EmailAddress emailAddress { get; set; }
            }

            public class EmailAddress
            {
                public string name { get; set; }
                public string address { get; set; }
            }
        }

    }
}
