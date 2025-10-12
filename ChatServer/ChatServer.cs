using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Text.Json;

namespace ChatServer
{
    public class ChatServer
    {
        private TcpListener _listener;
        private List<TcpClient> _clients = new List<TcpClient>();
        private List<User> _onlineUsers = new List<User>();
        private List<ChatMessage> _chatMessages = new List<ChatMessage>();
        private Dictionary<string, string> _users = new Dictionary<string, string>
        {
            {"user1", "password1"},
            {"admin", "admin123"},
            {"test", "test"}
        };

        public ChatServer(string ip, int port)
        {
            _listener = new TcpListener(IPAddress.Parse(ip), port);
        }

        public async Task StartAsync()
        {
            _listener.Start();
            Console.WriteLine("Сервер запущен...");

            while (true)
            {
                var client = await _listener.AcceptTcpClientAsync();
                _clients.Add(client);
                _ = Task.Run(() => HandleClientAsync(client));
            }
        }

        private async Task HandleClientAsync(TcpClient client)
        {
            var stream = client.GetStream();
            var buffer = new byte[4096];

            try
            {
                while (true)
                {
                    var bytesRead = await stream.ReadAsync(buffer, 0, buffer.Length);
                    if (bytesRead == 0) break;

                    var message = Encoding.UTF8.GetString(buffer, 0, bytesRead);
                    await ProcessMessageAsync(message, client);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Ошибка: {ex.Message}");
            }
            finally
            {
                _clients.Remove(client);
                client.Close();
            }
        }

        private async Task ProcessMessageAsync(string message, TcpClient client)
        {
            try
            {
                var messageObj = JsonSerializer.Deserialize<ClientMessage>(message);

                switch (messageObj.Type)
                {
                    case "Login":
                        await HandleLoginAsync(messageObj, client);
                        break;
                    case "Register":
                        await HandleRegisterAsync(messageObj, client);
                        break;
                    case "SendMessage":
                        await HandleSendMessageAsync(messageObj);
                        break;
                    case "GetOnlineUsers":
                        await SendOnlineUsersAsync(client);
                        break;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Ошибка обработки сообщения: {ex.Message}");
            }
        }

        private async Task HandleLoginAsync(ClientMessage message, TcpClient client)
        {
            var response = new ServerMessage { Type = "LoginResponse" };

            if (_users.TryGetValue(message.Username, out string storedPassword) && storedPassword == message.Password)
            {
                response.Success = true;
                response.Message = "Успешный вход!";

                // Добавляем пользователя в онлайн
                var user = new User { UserName = message.Username };
                if (!_onlineUsers.Exists(u => u.UserName == message.Username))
                {
                    _onlineUsers.Add(user);
                }

                // Отправляем историю сообщений и список пользователей
                await SendChatHistoryAsync(client);
                await BroadcastOnlineUsersAsync();
            }
            else
            {
                response.Success = false;
                response.Message = "Неверный логин или пароль";
            }

            await SendToClientAsync(client, response);
        }

        private async Task HandleRegisterAsync(ClientMessage message, TcpClient client)
        {
            var response = new ServerMessage { Type = "RegisterResponse" };

            if (!_users.ContainsKey(message.Username))
            {
                _users.Add(message.Username, message.Password);
                response.Success = true;
                response.Message = "Пользователь зарегистрирован!";

                // Добавляем пользователя в онлайн
                var user = new User { UserName = message.Username };
                _onlineUsers.Add(user);
                await BroadcastOnlineUsersAsync();
            }
            else
            {
                response.Success = false;
                response.Message = "Пользователь уже существует";
            }

            await SendToClientAsync(client, response);
        }

        private async Task HandleSendMessageAsync(ClientMessage message)
        {
            var chatMessage = new ChatMessage
            {
                Message = message.Message,
                Sender = message.Username,
                Timestamp = DateTime.Now,
                MessageType = "Message"
            };

            _chatMessages.Add(chatMessage);

            // Рассылаем сообщение всем клиентам
            var serverMessage = new ServerMessage
            {
                Type = "NewMessage",
                ChatMessage = chatMessage
            };

            await BroadcastToAllAsync(serverMessage);
        }

        private async Task SendChatHistoryAsync(TcpClient client)
        {
            var message = new ServerMessage
            {
                Type = "ChatHistory",
                ChatMessages = _chatMessages,
                OnlineUsers = _onlineUsers
            };

            await SendToClientAsync(client, message);
        }

        private async Task SendOnlineUsersAsync(TcpClient client)
        {
            var message = new ServerMessage
            {
                Type = "OnlineUsers",
                OnlineUsers = _onlineUsers
            };

            await SendToClientAsync(client, message);
        }

        private async Task BroadcastOnlineUsersAsync()
        {
            var message = new ServerMessage
            {
                Type = "OnlineUsers",
                OnlineUsers = _onlineUsers
            };

            await BroadcastToAllAsync(message);
        }

        private async Task BroadcastToAllAsync(ServerMessage message)
        {
            var json = JsonSerializer.Serialize(message);
            var data = Encoding.UTF8.GetBytes(json);

            foreach (var client in _clients.ToArray())
            {
                try
                {
                    await client.GetStream().WriteAsync(data, 0, data.Length);
                }
                catch
                {
                    // Клиент отключился
                }
            }
        }

        private async Task SendToClientAsync(TcpClient client, ServerMessage message)
        {
            var json = JsonSerializer.Serialize(message);
            var data = Encoding.UTF8.GetBytes(json);
            await client.GetStream().WriteAsync(data, 0, data.Length);
        }
    }

    public class ClientMessage
    {
        public string Type { get; set; } // "Login", "Register", "SendMessage", "GetOnlineUsers"
        public string Username { get; set; }
        public string Password { get; set; }
        public string Message { get; set; }
    }

    public class ServerMessage
    {
        public string Type { get; set; } // "LoginResponse", "RegisterResponse", "NewMessage", "ChatHistory", "OnlineUsers"
        public bool Success { get; set; }
        public string Message { get; set; }
        public ChatMessage ChatMessage { get; set; }
        public List<ChatMessage> ChatMessages { get; set; }
        public List<User> OnlineUsers { get; set; }
    }
}
