using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Table;

namespace GraphLogAnalyszeApp
{
    public class ConversationHistoryTableStorage : TableEntity
    {
        public string To { get; set; }
        public string From { get; set; }
    }
}
