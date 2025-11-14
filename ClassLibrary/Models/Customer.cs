using Azure;
using Azure.Data.Tables;
using System.ComponentModel.DataAnnotations;
using System.Diagnostics.Contracts;
using System.Diagnostics.Tracing;
using System.Transactions;

namespace ClassLibrary.Models
{
    public class Customer : ITableEntity
    {
        //---------------------------------------------------------------------------------------------------------------------
        [Key]
        public int CustomerID { get; set; } //must exist

        //---------------------------------------------------------------------------------------------------------------------
        public string? CustomerName { get; set; } //must exist

        //---------------------------------------------------------------------------------------------------------------------
        public string? CustomerEmail { get; set; }

        //---------------------------------------------------------------------------------------------------------------------
        public int? PhoneNumber { get; set; }

        //---------------------------------------------------------------------------------------------------------------------
        public string? AddressLine1 { get; set; }

        //---------------------------------------------------------------------------------------------------------------------
        public string? AddressLine2 { get; set; }

        //---------------------------------------------------------------------------------------------------------------------
        public string? Password{ get; set; }

        //---------------------------------------------------------------------------------------------------------------------
        //ITableEntity implementation
        public string PartitionKey { get; set; }
        public string RowKey { get; set; }
        public ETag ETag { get; set; }
        public DateTimeOffset? Timestamp { get; set; }
        //---------------------------------------------------------------------------------------------------------------------

    }
}
//----------------------------------------------------END OF FILE--------------------------------------------------------------