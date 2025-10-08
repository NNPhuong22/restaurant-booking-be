using System.ComponentModel.DataAnnotations;

namespace TableOrder_Hust.Models
{
    public class User
    {
        [Required]
        [Key]
        public string Email { get; set; } = string.Empty;
        public string FullName { get; set; } = string.Empty;

        public string Password { get; set; } = string.Empty;

        public string? GoogleId { get; set; }

        public string? Provider { get; set; }

        public string Role { get; set; } = "Customer";



    }
}
