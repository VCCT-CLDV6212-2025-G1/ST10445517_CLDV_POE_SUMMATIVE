using Azure;
using Azure.Data.Tables;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ClassLibrary.Models
{
    public class OrderItem : ITableEntity
    {
        //---------------------------------------------------------------------------------------------------------------------
        // ITableEntity Implementation
        public string? PartitionKey { get; set; }
        public string? RowKey { get; set; }
        public DateTimeOffset? Timestamp { get; set; }
        public ETag ETag { get; set; }

        //---------------------------------------------------------------------------------------------------------------------
        [Required(ErrorMessage = "Order reference is required")]
        public string? OrderRowKey { get; set; }

        //---------------------------------------------------------------------------------------------------------------------
        [Required(ErrorMessage = "Product reference is required")]
        public string? ProductRowKey { get; set; }

        //---------------------------------------------------------------------------------------------------------------------
        [Required(ErrorMessage = "Price is required")]
        public double CurrentPrice { get; set; }

        [Required(ErrorMessage = "Please select a quantity")]
        [Range(1, int.MaxValue, ErrorMessage = "Quantity must be at least 1")]
        public int Quantity { get; set; }

        //---------------------------------------------------------------------------------------------------------------------
        [NotMapped]
        public decimal Subtotal => (decimal)CurrentPrice * Quantity;

        //---------------------------------------------------------------------------------------------------------------------
        [NotMapped]
        public Product? Product { get; set; }
    }
}
//----------------------------------------------------END OF FILE--------------------------------------------------------------