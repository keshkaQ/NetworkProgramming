namespace ServerChat.Models
{
    public class ChatMessage
    {
        public string Message { get; set; }
        public string Sender { get; set; }
        public string Receiver { get; set; }
        public string RoomId { get; set; }
        public string RoomName { get; set; }
        public DateTime Timestamp { get; set; }
    }

}
