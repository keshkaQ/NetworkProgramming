using ServerChat.Models;
using System.Collections.ObjectModel;
using System.Net.Sockets;

namespace ServerChat.Managers
{
    public class UserManager
    {
        private readonly Dictionary<string, string> _users;
        private readonly ObservableCollection<User> _onlineUsers;
        public readonly Dictionary<TcpClient, string> _clientUsers;
        public UserManager()
        {
            _users = new Dictionary<string, string>
            {
                {"user1", "password1"},
                {"admin", "admin123"},
                {"test", "test"}
            };
            _onlineUsers = new ObservableCollection<User>();
            _clientUsers = new Dictionary<TcpClient, string>();
        }

        public bool AuthenticateUser(string username, string password)
        {
            return _users.TryGetValue(username, out string storedPassword) && storedPassword == password;
        }

        public bool RegisterUser(string username, string password)
        {
            if (_users.ContainsKey(username)) return false;

            _users.Add(username, password);
            return true;
        }

        public void AddOnlineUser(string username, TcpClient client)
        {
            if (!_onlineUsers.Any(u => u.UserName == username))
            {
                _onlineUsers.Add(new User { UserName = username });
            }
            _clientUsers[client] = username;
        }

        public void RemoveOnlineUser(string username, TcpClient client)
        {
            var userToRemove = _onlineUsers.FirstOrDefault(u => u.UserName == username);
            if (userToRemove != null)
            {
                _onlineUsers.Remove(userToRemove);
            }
            _clientUsers.Remove(client);
        }

        public string GetOnlineUsersString()
        {
            return string.Join(",", _onlineUsers.Select(u => u.UserName));
        }

        public string GetUsernameByClient(TcpClient client)
        {
            return _clientUsers.TryGetValue(client, out string username) ? username : null;
        }

        public TcpClient GetClientByUsername(string username)
        {
            return _clientUsers.FirstOrDefault(x => x.Value == username).Key;
        }
    }
}
