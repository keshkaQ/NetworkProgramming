namespace Producer.Models
{
    public class OrderMessage
    {
        public string? ModelName { get; set; }
        public string? Color { get; set; }
        public string? Storage { get; set; }
        public string? SimType { get; set; }
        public string? CustomerName { get; set; }
        public string? CustomerEmail { get; set; }
        public decimal FinalPrice { get; set; }
        public DateTime OrderDate { get; set; }
        public decimal BasePrice { get; set; }
        public string? ImageSource { get; set; }
        public string? Specifications { get; set; }
    }
}
