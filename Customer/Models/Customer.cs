using System.ComponentModel.DataAnnotations;

namespace Consumer.Models
{
    public class Customer
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public string? Name { get; set; }

        [EmailAddress]
        public string? Email { get; set; }
    }
}
