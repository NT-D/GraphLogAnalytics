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
        }

    }
}
