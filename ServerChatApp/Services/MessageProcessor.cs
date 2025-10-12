using ClientChatApp.Models;
using System.Windows.Media;

namespace ClientChatApp.Services
{
    public class MessageProcessor
    {
        private readonly Dictionary<string, SolidColorBrush> _userColors = new();

        public SolidColorBrush GetOrCreateUserColor(string username)
        {
            if (_userColors.TryGetValue(username, out var color))
                return color;

            var hash = username.GetHashCode();
            var r = (byte)((hash >> 16) % 56 + 200);
            var g = (byte)((hash >> 8) % 56 + 200);
            var b = (byte)(hash % 56 + 200);
            var newColor = new SolidColorBrush(Color.FromRgb(r, g, b));
            _userColors[username] = newColor;
            return newColor;
        }

        public ChatMessage CreatePublicMessage(string sender, string messageText)
        {
            var color = GetOrCreateUserColor(sender);
            return new ChatMessage
            {
                Sender = sender,
                Message = messageText,
                Timestamp = DateTime.Now,
                Background = color,
                TextColor = Brushes.Black,
                SenderColor = Brushes.DarkBlue,
                IsPrivate = false
            };
        }
        public ChatMessage CreateRoomMessage(string sender, string roomId, string message, string roomName)
        {
            return new ChatMessage
            {
                Message = message,
                Sender = sender,
                RoomId = roomId,
                RoomName = roomName,
                Timestamp = DateTime.Now,
                Background = Brushes.LightCyan,
                TextColor = Brushes.Black,
                SenderColor = Brushes.DarkCyan,
                IsPrivate = false,
                IsRoomMessage = true
            };
        }

        public ChatMessage CreatePrivateMessage(string sender, string receiver, string messageText, string currentUser)
        {
            Brush bg = sender == currentUser ? Brushes.LightGreen : Brushes.LightYellow;
            Brush sc = sender == currentUser ? Brushes.DarkGreen : Brushes.DarkOrange;

            return new ChatMessage
            {
                Sender = sender,
                Receiver = receiver,
                Message = messageText,
                Timestamp = DateTime.Now,
                Background = bg,
                TextColor = Brushes.Black,
                SenderColor = sc,
                IsPrivate = true 
            };
        }
    }
}
