using System.ComponentModel.DataAnnotations;

namespace Consumer.Models
{
    public class Order
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public string? iPhoneModel { get; set; }
        public string? Color { get; set; }
        public string? Storage { get; set; }
        public string? SimType { get; set; }
        public string? Price { get; set; }
        public DateTime OrderDate { get; set; }

        // Внешний ключ для связи с клиентом
        public int CustomerId { get; set; }
    }

}
