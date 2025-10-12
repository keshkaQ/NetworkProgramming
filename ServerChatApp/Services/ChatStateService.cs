using ClientChatApp.Models;
using System.Collections.ObjectModel;

namespace ClientChatApp.Services
{
    public class ChatStateService
    {
        public string? CurrentUser { get; set; }
        public string? CurrentChatMode { get; set; } = "public";
        public string? PrivateChatUser { get; set; }
        public string? PreviousChatTitle { get; set; } = "Общий чат"; 
        public ObservableCollection<User> OnlineUsers { get; } = new ObservableCollection<User>();
        public List<ChatMessage> AllMessages { get; } = new List<ChatMessage>();

        public void Reset()
        {
            CurrentUser = null;
            CurrentChatMode = "public";
            PrivateChatUser = null;
            PreviousChatTitle = "Общий чат";
            OnlineUsers.Clear();
            AllMessages.Clear();
        }
    }
}
