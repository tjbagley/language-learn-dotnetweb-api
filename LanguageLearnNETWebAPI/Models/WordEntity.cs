using Azure;
using Azure.Data.Tables;

namespace LanguageLearnNETWebAPI.Models
{
    public class WordEntity : WordBase, ITableEntity
    {
        public string PartitionKey { get; set; } = string.Empty;
        public string RowKey { get; set; } = string.Empty;
        public DateTimeOffset? Timestamp { get; set; }
        public ETag ETag { get; set; }
    }
}
