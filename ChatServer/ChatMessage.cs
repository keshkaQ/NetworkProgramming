namespace ChatServer
{
    public class ChatMessage
    {
        public string Message { get; set; }
        public string Sender { get; set; }
        public DateTime Timestamp { get; set; }
        public string MessageType { get; set; } 
    }
}
