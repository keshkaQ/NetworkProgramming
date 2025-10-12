using ServerChat.Managers;
using System.IO;
using System.Net.Sockets;
using System.Text;

namespace ServerChat.Handlers
{
    public class ClientHandler
    {
        private readonly UserManager _userManager;
        private readonly RoomManager _roomManager;
        private readonly MessageManager _messageManager;
        private event Action<string> _onLogMessage;
        private readonly Action _updateClientsCount;

        public ClientHandler(UserManager userManager, RoomManager roomManager, 
                             MessageManager messageManager, Action<string> onLogMessage, Action updateClientsCount)
        {
            _userManager = userManager;
            _messageManager = messageManager;
            _roomManager = roomManager;
            _onLogMessage = onLogMessage;
            _updateClientsCount = updateClientsCount;
        }

        public async Task HandleClientAsync(TcpClient client)
        {
            NetworkStream stream = null;

            try
            {
                stream = client.GetStream();
                var buffer = new byte[4096];

                while (client.Connected)
                {
                    var bytesRead = await stream.ReadAsync(buffer, 0, buffer.Length);
                    if (bytesRead == 0) break;

                    var message = Encoding.UTF8.GetString(buffer, 0, bytesRead).Trim();
                    if (!string.IsNullOrEmpty(message))
                    {
                        await ProcessClientMessageAsync(message, client);
                    }
                }
            }
            catch (IOException ioEx) when (ioEx.InnerException is SocketException socketEx)
            {
                _onLogMessage?.Invoke($"🔌 Клиент отключился: {socketEx.SocketErrorCode}");
            }
            catch (Exception ex)
            {
                _onLogMessage?.Invoke($"📨 Ошибка обработки клиента: {ex.Message}");
            }
            finally
            {
                var username = _userManager.GetUsernameByClient(client);
                if (username != null)
                {
                    await HandleLeaveRoomAsync(username, client, true);
                    _userManager.RemoveOnlineUser(username, client);
                    _onLogMessage?.Invoke($"👋 Пользователь {username} отключился");
                    await BroadcastOnlineUsersAsync();
                }

                try
                {
                    stream?.Close();
                    client?.Close();
                    _onLogMessage?.Invoke($"🔌 Клиент отключен");
                    _updateClientsCount?.Invoke();
                }
                catch (Exception ex)
                {
                    _onLogMessage?.Invoke($"❌ Ошибка при закрытии соединения: {ex.Message}");
                }
            }
        }
        private async Task ProcessClientMessageAsync(string message, TcpClient client)
        {
            try
            {
                var parts = message.Split('|');
                if (parts.Length < 2) return;

                var type = parts[0];
                var username = parts.Length > 1 ? parts[1] : "";

                _onLogMessage?.Invoke($"📨 Получено: {type} от {username}");

                switch (type)
                {
                    case "LOGIN":
                        if (parts.Length >= 3)
                        {
                            var password = parts[2];
                            await HandleLoginAsync(username, password, client);
                        }
                        break;
                    case "REGISTER":
                        if (parts.Length >= 3)
                        {
                            var password = parts[2];
                            await HandleRegisterAsync(username, password, client);
                        }
                        break;
                    case "MESSAGE":
                        if (parts.Length >= 3)
                        {
                            var messageText = parts[2];
                            await HandleSendMessageAsync(username, messageText);
                        }
                        break;
                    case "PRIVATE_MESSAGE":
                        if (parts.Length >= 4)
                        {
                            var receiver = parts[2];
                            var messageText = parts[3];
                            await HandlePrivateMessageAsync(username, receiver, messageText);
                        }
                        break;
                    case "LOGOUT": 
                        if (parts.Length >= 2)
                        {
                            await HandleLogoutAsync(username, client);
                            client.Close(); 
                            return;
                        }
                        break;
                    case "CREATE_ROOM":
                        if (parts.Length >= 3)
                        {
                            var roomName = parts[2];
                            await HandleCreateRoomAsync(username, roomName, client);
                        }
                        break;
                    case "JOIN_ROOM":
                        if (parts.Length >= 3)
                        {
                            var roomId = parts[2];
                            await HandleJoinRoomAsync(username, roomId, client);
                        }
                        break;
                    case "LEAVE_ROOM":
                        if (parts.Length >= 2)
                        {
                            await HandleLeaveRoomAsync(username, client);
                        }
                        break;
                    case "ROOM_MESSAGE":
                        if (parts.Length >= 4)
                        {
                            var roomId = parts[2];
                            var messageText = parts[3];
                            await HandleRoomMessageAsync(username, roomId, messageText);
                        }
                        break;
                    case "GET_ROOMS":
                        if (parts.Length >= 2)
                        {
                            await HandleGetRoomsAsync(client);
                        }
                        break;
                }
            }
            catch (Exception ex)
            {
                _onLogMessage?.Invoke($"❌ Ошибка обработки сообщения: {ex.Message}");
            }
        }
        private async Task HandleCreateRoomAsync(string username, string roomName, TcpClient client)
        {
            try
            {
                var room = _roomManager.CreateRoom(roomName, username);
                if (room != null)
                {
                    await SendToClientAsync(client, $"ROOM_CREATED|{room.RoomId}");
                    _onLogMessage?.Invoke($"🏠 Пользователь {username} создал комнату '{roomName}'");

                    // Автоматически добавляем создателя в комнату
                    await HandleJoinRoomAsync(username, room.RoomId, client);

                    // Обновляем список комнат у всех клиентов
                    await BroadcastRoomListAsync();
                }
                else
                {
                    await SendToClientAsync(client, "ROOM_CREATE_FAILED|Комната с таким именем уже существует");
                }
            }
            catch (Exception ex)
            {
                await SendToClientAsync(client, $"ROOM_CREATE_FAILED|Ошибка создания комнаты: {ex.Message}");
            }
        }

        private async Task HandleJoinRoomAsync(string username, string roomId, TcpClient client)
        {
            try
            {
                var room = _roomManager.GetRoom(roomId);
                if (room == null)
                {
                    await SendToClientAsync(client, "ROOM_JOIN_FAILED|Комната не найдена");
                    return;
                }

                if (_roomManager.AddUserToRoom(roomId, username))
                {
                    // Выходим из предыдущей комнаты если был в одной
                    var currentRoom = _roomManager.GetUserRoom(username);
                    if (currentRoom != null && currentRoom.RoomId != roomId)
                    {
                        _roomManager.RemoveUserFromRoom(currentRoom.RoomId, username);
                        await BroadcastToRoomAsync(currentRoom.RoomId, $"ROOM_USER_LEFT|{username}");
                        await BroadcastRoomListAsync();
                    }

                    await SendToClientAsync(client, $"ROOM_JOINED|{room.RoomId}|{room.RoomName}");

                    // Уведомляем других участников комнаты
                    await BroadcastToRoomAsync(roomId, $"ROOM_USER_JOINED|{username}", username);

                    // Отправляем историю сообщений комнаты
                    await SendRoomHistoryAsync(client, roomId);

                    _onLogMessage?.Invoke($"🚪 Пользователь {username} вошел в комнату '{room.RoomName}'");

                    // Обновляем список комнат
                    await BroadcastRoomListAsync();
                }
                else
                {
                    await SendToClientAsync(client, "ROOM_JOIN_FAILED|Не удалось войти в комнату");
                }
            }
            catch (Exception ex)
            {
                await SendToClientAsync(client, $"ROOM_JOIN_FAILED|Ошибка входа в комнату: {ex.Message}");
            }
        }

        private async Task HandleLeaveRoomAsync(string username, TcpClient client, bool isDisconnecting = false)
        {
            try
            {
                var room = _roomManager.GetUserRoom(username);
                if (room != null)
                {
                    _roomManager.RemoveUserFromRoom(room.RoomId, username);

                    if (!isDisconnecting)
                    {
                        await SendToClientAsync(client, "ROOM_LEFT");
                        _onLogMessage?.Invoke($"📤 Отправлен ROOM_LEFT пользователю {username}");
                    }

                    // Уведомляем других участников комнаты
                    await BroadcastToRoomAsync(room.RoomId, $"ROOM_USER_LEFT|{username}");

                    // Если комната пустая - удаляем ее
                    if (room.Users.Count == 0)
                    {
                        _roomManager.RemoveRoom(room.RoomId);
                        _onLogMessage?.Invoke($"🗑️ Комната '{room.RoomName}' удалена (нет участников)");
                    }

                    _onLogMessage?.Invoke($"🚶 Пользователь {username} вышел из комнаты '{room.RoomName}'");

                    // Обновляем список комнат
                    await BroadcastRoomListAsync();
                }
            }
            catch (Exception ex)
            {
                _onLogMessage?.Invoke($"❌ Ошибка выхода из комнаты: {ex.Message}");
            }
        }

        private async Task HandleRoomMessageAsync(string username, string roomId, string messageText)
        {
            try
            {
                var room = _roomManager.GetRoom(roomId);
                if (room == null || !room.Users.Contains(username))
                {
                    return; // Пользователь не в комнате или комната не существует
                }

                // Сохраняем сообщение комнаты
                _messageManager.AddRoomMessage(username, roomId, messageText);

                var message = $"ROOM_MESSAGE|{username}|{roomId}|{room.RoomName}|{DateTime.Now:HH:mm}|{messageText}";
                await BroadcastToRoomAsync(roomId, message);

                _onLogMessage?.Invoke($"💬 {username} в комнате '{room.RoomName}': {messageText}");
            }
            catch (Exception ex)
            {
                _onLogMessage?.Invoke($"❌ Ошибка отправки сообщения в комнату: {ex.Message}");
            }
        }

        private async Task HandleGetRoomsAsync(TcpClient client)
        {
            try
            {
                var rooms = _roomManager.GetAllRooms();
                var roomsData = string.Join(";", rooms.Select(r => $"{r.RoomId}:{r.RoomName}:{r.Users.Count}"));
                await SendToClientAsync(client, $"ROOM_LIST|{roomsData}");
            }
            catch (Exception ex)
            {
                _onLogMessage?.Invoke($"❌ Ошибка получения списка комнат: {ex.Message}");
            }
        }

        private async Task BroadcastToRoomAsync(string roomId, string message, string excludeUser = null)
        {
            var room = _roomManager.GetRoom(roomId);
            if (room == null) return;

            var data = Encoding.UTF8.GetBytes(message + "\n");

            foreach (var username in room.Users)
            {
                if (username == excludeUser) continue;

                var client = _userManager.GetClientByUsername(username);
                if (client != null && client.Connected)
                {
                    try
                    {
                        var stream = client.GetStream();
                        await stream.WriteAsync(data, 0, data.Length);
                        await stream.FlushAsync();
                    }
                    catch (Exception ex)
                    {
                        _onLogMessage?.Invoke($"❌ Ошибка отправки в комнату пользователю {username}: {ex.Message}");
                    }
                }
            }
        }

        private async Task BroadcastRoomListAsync()
        {
            var rooms = _roomManager.GetAllRooms();
            var roomsData = string.Join(";", rooms.Select(r => $"{r.RoomId}:{r.RoomName}:{r.Users.Count}"));
            var message = $"ROOM_LIST|{roomsData}";

            // Рассылаем всем подключенным клиентам
            await BroadcastToAllAsync(message);
        }

        private async Task SendRoomHistoryAsync(TcpClient client, string roomId)
        {
            var currentUser = _userManager.GetUsernameByClient(client);
            if (currentUser == null) return;

            foreach (var msg in _messageManager.GetRoomMessages(roomId))
            {
                var message = $"ROOM_MESSAGE|{msg.Sender}|{roomId}|{msg.RoomName}|{msg.Timestamp:HH:mm}|{msg.Message}";
                await SendToClientAsync(client, message);
                await Task.Delay(10);
            }
        }


        private async Task HandleLoginAsync(string username, string password, TcpClient client)
        {
            if (_userManager.AuthenticateUser(username, password))
            {
                await SendToClientAsync(client, "LOGIN_SUCCESS|Успешный вход!");
                _userManager.AddOnlineUser(username, client);
                _onLogMessage?.Invoke($"👤 Пользователь {username} вошел в чат");
                _updateClientsCount?.Invoke();
                await SendChatHistoryAsync(client);
                await BroadcastOnlineUsersAsync();
            }
            else
            {
                await SendToClientAsync(client, "LOGIN_FAILED|Неверный логин или пароль");
            }
        }

        private async Task HandleRegisterAsync(string username, string password, TcpClient client)
        {
            if (_userManager.RegisterUser(username, password))
            {
                await SendToClientAsync(client, "REGISTER_SUCCESS|Пользователь зарегистрирован!");
                _userManager.AddOnlineUser(username, client);
                _onLogMessage?.Invoke($"👤 Новый пользователь {username} зарегистрирован");
                _updateClientsCount?.Invoke();

                await SendChatHistoryAsync(client);
                await BroadcastOnlineUsersAsync();
            }
            else
            {
                await SendToClientAsync(client, "REGISTER_FAILED|Пользователь уже существует");
            }
        }

        private async Task HandleSendMessageAsync(string username, string messageText)
        {
            _messageManager.AddMessage(username, messageText);

            var message = $"NEW_MESSAGE|{username}|{DateTime.Now:HH:mm}|{messageText}";
            await BroadcastToAllAsync(message);

            _onLogMessage?.Invoke($"💬 {username}: {messageText}");
        }

        private async Task HandlePrivateMessageAsync(string sender, string receiver, string messageText)
        {
            var senderClient = _userManager.GetClientByUsername(sender);
            var receiverClient = _userManager.GetClientByUsername(receiver);

            var message = $"PRIVATE_MESSAGE|{sender}|{receiver}|{DateTime.Now:HH:mm}|{messageText}";

            if (senderClient != null && senderClient.Connected)
            {
                await SendToClientAsync(senderClient, message);
            }

            if (receiverClient != null && receiverClient.Connected)
            {
                await SendToClientAsync(receiverClient, message);
            }

            _onLogMessage?.Invoke($"🔒 {sender} -> {receiver}: {messageText}");
        }

        private async Task HandleLogoutAsync(string username, TcpClient client)
        {
            _userManager.RemoveOnlineUser(username, client);
            _onLogMessage?.Invoke($"👋 Пользователь {username} вышел из чата");
            _updateClientsCount?.Invoke();
            await BroadcastOnlineUsersAsync();
        }

        private async Task SendChatHistoryAsync(TcpClient client)
        {
            var currentUser = _userManager.GetUsernameByClient(client);
            if (currentUser == null) return;

            foreach (var msg in _messageManager.GetUserMessages(currentUser))
            {
                var message = $"NEW_MESSAGE|{msg.Sender}|{msg.Timestamp:HH:mm}|{msg.Message}";
                await SendToClientAsync(client, message);
            }
        }

        private async Task BroadcastOnlineUsersAsync()
        {
            var users = _userManager.GetOnlineUsersString();
            await BroadcastToAllAsync($"ONLINE_USERS|{users}");
        }

        private async Task BroadcastToAllAsync(string message)
        {
            var data = Encoding.UTF8.GetBytes(message + "\n");

            foreach (var client in _userManager._clientUsers.Keys.ToArray())
            {
                try
                {
                    if (client.Connected)
                    {
                        var stream = client.GetStream();
                        await stream.WriteAsync(data, 0, data.Length);
                        await stream.FlushAsync();
                    }
                }
                catch (Exception ex)
                {
                    _onLogMessage?.Invoke($"❌ Ошибка отправки клиенту: {ex.Message}");
                }
            }
        }

        private async Task SendToClientAsync(TcpClient client, string message)
        {
            try
            {
                if (client?.Connected == false) return;

                var data = Encoding.UTF8.GetBytes(message + "\n");
                var stream = client.GetStream();
                await stream.WriteAsync(data, 0, data.Length);
                await stream.FlushAsync();
            }
            catch (Exception ex)
            {
                _onLogMessage?.Invoke($"❌ Ошибка отправки клиенту: {ex.Message}");
            }
        }
    }
}