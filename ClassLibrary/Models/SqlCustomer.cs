using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ClassLibrary.Models
{
    [Table("SqlCustomers")]
    public class SqlCustomer
    {
        //---------------------------------------------------------------------------------------------------------------------
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int CustomerID { get; set; }

        //---------------------------------------------------------------------------------------------------------------------
        [Required]
        public string? CustomerName { get; set; }

        //---------------------------------------------------------------------------------------------------------------------
        [Required]
        public string? CustomerEmail { get; set; }

        //---------------------------------------------------------------------------------------------------------------------
        [Required]
        public string? PasswordHash { get; set; }

        //---------------------------------------------------------------------------------------------------------------------
        [Required]
        public string Role { get; set; } = "Customer"; // "Customer" or "Admin"
        //---------------------------------------------------------------------------------------------------------------------
    }
}
//----------------------------------------------------END OF FILE--------------------------------------------------------------