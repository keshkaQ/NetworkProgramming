using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Consumer.Models
{
    public class iPhone
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public string? Name { get; set; }
        public string? BaseModelName { get; set; }
        public string? Color { get; set; }
        public string? Storage { get; set; }
        public string? Price { get; set; }
        public string? OldPrice { get; set; }
        public string? Rating { get; set; }
        public string? Reviews { get; set; }
        public string? Specifications { get; set; }
        public string? ImageSource { get; set; }
    }
}
