namespace Consumer.Models
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
    }
}
