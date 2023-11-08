using System.ComponentModel.DataAnnotations;

namespace Crypto_MVC.Data.Models
{
    public class Subscription
    {
        [Key]
        public Guid Id { get; set; }

        [Required]
        public string Email { get; set; } = null!;

        [Required]
        public double Percentage { get; set; }

        [Required]
        public double Hours { get; set; }

        public bool IsDeleted { get; set; }
    }
}
