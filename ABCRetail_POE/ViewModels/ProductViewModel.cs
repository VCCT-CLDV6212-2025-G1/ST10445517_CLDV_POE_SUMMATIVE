using ClassLibrary.Models;
using Microsoft.AspNetCore.Http;
using System.ComponentModel.DataAnnotations;

namespace ABCRetail_POE.ViewModels
{
    //---------------------------------------------------------------------------------------------------------------------
    public class ProductViewModel
    {
        //---------------------------------------------------------------------------------------------------------------------
        [Required]
        public string? ProductName { get; set; }

        //---------------------------------------------------------------------------------------------------------------------
        [Required]
        public string? ProductDescription { get; set; }

        //---------------------------------------------------------------------------------------------------------------------
        [Required]
        [Range(0.01, 100000.00, ErrorMessage = "Price must be greater than zero :(")]
        public decimal Price { get; set; }

        //---------------------------------------------------------------------------------------------------------------------
        [Required]
        [Range(1, 10000, ErrorMessage = "Stock quantity must be at least 1 :(")]
        public int StockQuantity { get; set; }

        //---------------------------------------------------------------------------------------------------------------------
        // used for the actual file upload
        [Required(ErrorMessage = "Please select an image file :(")]
        [Display(Name = "Product Image")]
        public IFormFile? ImageFile { get; set; }

        //---------------------------------------------------------------------------------------------------------------------
        public string? ImageUrl { get; set; }
        //---------------------------------------------------------------------------------------------------------------------
    }
}
//------------------------------------------------------END OF FILE------------------------------------------------------------