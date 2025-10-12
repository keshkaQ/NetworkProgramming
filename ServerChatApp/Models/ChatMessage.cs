using System.Windows.Media;

namespace ClientChatApp.Models
{
    public class ChatMessage
    {
        public string? Message { get; set; }
        public string? Sender { get; set; }
        public string? Receiver { get; set; }
        public string? RoomId { get; set; }
        public string? RoomName { get; set; }
        public DateTime Timestamp { get; set; }
        public Brush? Background { get; set; }
        public Brush? TextColor { get; set; }
        public Brush? SenderColor { get; set; }
        public bool IsPrivate { get; set; }
        public bool IsRoomMessage { get; set; }
    }
}
