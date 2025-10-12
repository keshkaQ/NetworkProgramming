using ServerChat.Models;
using System.Collections.Concurrent;

namespace ServerChat.Managers
{
    public class MessageManager
    {
        private readonly ConcurrentBag<ChatMessage> _chatMessages = new();
        private readonly ConcurrentDictionary<string, ConcurrentBag<ChatMessage>> _roomMessages = new();

        public void AddMessage(string sender, string message, string receiver = "")
        {
            _chatMessages.Add(new ChatMessage
            {
                Message = message,
                Sender = sender,
                Receiver = receiver,
                Timestamp = DateTime.Now
            });
        }
        public void AddRoomMessage(string sender, string roomId, string message)
        {
            if (!_roomMessages.ContainsKey(roomId))
            {
                _roomMessages[roomId] = new ConcurrentBag<ChatMessage>();
            }

            _roomMessages[roomId].Add(new ChatMessage
            {
                Sender = sender,
                Message = message,
                RoomId = roomId,
                Timestamp = DateTime.Now
            });
        }
        public IEnumerable<ChatMessage> GetPublicMessages()
        {
            return _chatMessages.Where(m => string.IsNullOrEmpty(m.Receiver));
        }

        public IEnumerable<ChatMessage> GetUserMessages(string username)
        {
            return _chatMessages.Where(m =>
                string.IsNullOrEmpty(m.Receiver) || // Общие сообщения
                m.Sender == username || // Сообщения от пользователя
                m.Receiver == username  // Сообщения для пользователя
            );
        }
        public IEnumerable<ChatMessage> GetRoomMessages(string roomId)
        {
            return _roomMessages.TryGetValue(roomId, out var messages)
                ? messages.OrderBy(m => m.Timestamp)
                : Enumerable.Empty<ChatMessage>();
        }
    }
}
