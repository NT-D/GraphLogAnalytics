using Microsoft.WindowsAzure.Storage.Table;

namespace GraphLogAnalyzeApp.TableScheme
{
    public class EventsTable : TableEntity
    {
        public string To { get; set; }
        public string From { get; set; }
    }
}
