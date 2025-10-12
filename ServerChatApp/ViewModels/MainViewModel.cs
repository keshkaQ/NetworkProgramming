using ClientChatApp.Models;
using ClientChatApp.Services;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Input;

namespace ClientChatApp.ViewModels
{
    public class MainViewModel : INotifyPropertyChanged
    {
        private readonly NetworkService _network = new();
        private readonly MessageProcessor _processor = new();
        private readonly ChatStateService _state = new();

        public ObservableCollection<User> OnlineUsers => _state.OnlineUsers;
        public ObservableCollection<ChatMessage> DisplayedMessages { get; private set; }
        public ObservableCollection<Room> AvailableRooms { get; private set; }

        private string _loginText;
        private string _passwordText;
        private string _roomNumberText;
        private Room _selectedRoom;
        private User _selectedUser;
        private string _chatTitle = "Общий чат";
        private string _messageText;


        public bool IsConnected => _network.IsConnected;
        public bool IsAuthenticated => !string.IsNullOrEmpty(_state.CurrentUser) && _network.IsConnected;
        public bool CanSendMessage => IsConnected && !string.IsNullOrWhiteSpace(MessageText);
        public bool CanConnect => !IsConnected && !IsAuthenticated;
        public bool CanStartPrivateChat => IsConnected && SelectedUser != null && SelectedUser.UserName != _state.CurrentUser;
        public bool CanCreateRoom => IsAuthenticated;
        public bool CanJoinRoom => IsAuthenticated && _selectedRoom != null;
        public bool CanRegister => IsConnected && !IsAuthenticated && !string.IsNullOrWhiteSpace(LoginText) && !string.IsNullOrWhiteSpace(PasswordText);
        public bool CanLogin => IsConnected && !IsAuthenticated && !string.IsNullOrWhiteSpace(LoginText) && !string.IsNullOrWhiteSpace(PasswordText);

        // Свойства для привязки
        public string RoomNumberText
        {
            get => _roomNumberText;
            set
            {
                _roomNumberText = value;
                OnPropertyChanged();
            }
        }

        public string LoginText
        {
            get => _loginText;
            set
            {
                _loginText = value;
                OnPropertyChanged();
                ((RelayCommand)LoginCommand).RaiseCanExecuteChanged();
                ((RelayCommand)RegisterCommand).RaiseCanExecuteChanged();
            }
        }

        public string PasswordText
        {
            get => _passwordText;
            set
            {
                _passwordText = value;
                OnPropertyChanged();
                ((RelayCommand)LoginCommand).RaiseCanExecuteChanged();
                ((RelayCommand)RegisterCommand).RaiseCanExecuteChanged();
            }
        }

        public string MessageText
        {
            get => _messageText;
            set
            {
                _messageText = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(CanSendMessage));
                ((RelayCommand)SendMessageCommand).RaiseCanExecuteChanged();
            }
        }

        public Room SelectedRoom
        {
            get => _selectedRoom;
            set
            {
                _selectedRoom = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(CanJoinRoom));
                ((RelayCommand)JoinRoomCommand).RaiseCanExecuteChanged();
            }
        }


        public string ChatTitle
        {
            get => _chatTitle;
            set
            {
                _chatTitle = value;
                OnPropertyChanged();
            }
        }


        public User SelectedUser
        {
            get => _selectedUser;
            set
            {
                _selectedUser = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(CanStartPrivateChat));
                ((RelayCommand)PrivateChatCommand).RaiseCanExecuteChanged();
            }
        }

        public MainViewModel()
        {
            _network.OnMessageReceived += OnMessageReceived;
            _network.OnError += msg => MessageBox.Show(msg, "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);

            // Инициализация коллекций
            DisplayedMessages = new ObservableCollection<ChatMessage>();
            AvailableRooms = new ObservableCollection<Room>();

            InitializeCommands();
        }


        // Команды 
        public ICommand JoinRoomCommand { get; private set; }
        public ICommand LeaveRoomCommand {  get; private set; }
        public ICommand CreateRoomCommand { get; private set; }
        public ICommand ConnectCommand { get; private set; }
        public ICommand LoginCommand { get; private set; }
        public ICommand RegisterCommand { get; private set; }
        public ICommand ExitCommand { get; private set; }
        public ICommand SendMessageCommand { get; private set; }
        public ICommand ClearMessageCommand { get; private set; }
        public ICommand PrivateChatCommand { get; private set; }

        private void InitializeCommands()
        {
            ConnectCommand = new RelayCommand(_ => Connect(),_=>CanConnect);
            LoginCommand = new RelayCommand(_ => Login(), _ => CanLogin);
            RegisterCommand = new RelayCommand(_ => Register(), _ => CanRegister);
            ExitCommand = new RelayCommand(_ => Exit());
            SendMessageCommand = new RelayCommand(_ => SendMessage(), _ => CanSendMessage);
            ClearMessageCommand = new RelayCommand(_ => MessageText = "");
            PrivateChatCommand = new RelayCommand(TogglePrivateChat, _ => CanStartPrivateChat);
            CreateRoomCommand = new RelayCommand(_ => CreateRoom(),_=> CanCreateRoom);
            JoinRoomCommand = new RelayCommand(_ => JoinRoom(), _ => CanJoinRoom);
            LeaveRoomCommand = new RelayCommand(_ => LeaveRoom());
        }

        private async void Connect()
        {
            await _network.ConnectAsync("127.0.0.1", 8888);
            if (_network.IsConnected)
            {
                MessageBox.Show("Подключено!", "Успех", MessageBoxButton.OK, MessageBoxImage.Information);
                await _network.SendAsync($"GET_ROOMS|{_state.CurrentUser}");
            }
            UpdateAllProperties();
        }
        private void LeaveRoom()
        {
            _ = _network.SendAsync($"LEAVE_ROOM|{_state.CurrentUser}");
        }

        private void CreateRoom()
        {
            if (string.IsNullOrWhiteSpace(RoomNumberText)) return;

            _ = _network.SendAsync($"CREATE_ROOM|{_state.CurrentUser}|{RoomNumberText}");

        }
        private void JoinRoom()
        {
            if (SelectedRoom == null) return;

            _ = _network.SendAsync($"JOIN_ROOM|{_state.CurrentUser}|{SelectedRoom.RoomId}");
            ChatTitle = $"Комната: {SelectedRoom.RoomName}";
        }

        private void Login()
        {
            if (!CanLogin)
            {
                if (!IsConnected)
                {
                    MessageBox.Show("Сначала подключитесь к серверу", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
                }
                else if (IsAuthenticated)
                {
                    MessageBox.Show("Вы уже авторизованы!", "Информация", MessageBoxButton.OK, MessageBoxImage.Information);
                }
                else
                {
                    MessageBox.Show("Заполните все поля", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
                }
                return;
            }

            _state.CurrentUser = LoginText.Trim();
            _ = _network.SendAsync($"LOGIN|{LoginText}|{PasswordText}");
        }

        private void Register()
        {
            if (!CanRegister)
            {
                if (!IsConnected)
                {
                    MessageBox.Show("Сначала подключитесь к серверу", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
                }
                else if (IsAuthenticated)
                {
                    MessageBox.Show("Вы уже авторизованы!", "Информация", MessageBoxButton.OK, MessageBoxImage.Information);
                }
                else
                {
                    MessageBox.Show("Заполните все поля", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
                }
                return;
            }

            _state.CurrentUser = LoginText.Trim();
            _ = _network.SendAsync($"REGISTER|{LoginText}|{PasswordText}");
        }

        private async void Exit()
        {
            if (!string.IsNullOrEmpty(_state.CurrentUser) && _network.IsConnected)
                await _network.SendAsync($"LOGOUT|{_state.CurrentUser}");

            _state.Reset();
            _network.Disconnect();
            UpdateAllProperties();
        }

        private async void SendMessage()
        {
            if (string.IsNullOrWhiteSpace(MessageText)) return;
            var text = MessageText.Trim();

            if (_state.CurrentChatMode == "private" && !string.IsNullOrEmpty(_state.PrivateChatUser))
            {
                await _network.SendAsync($"PRIVATE_MESSAGE|{_state.CurrentUser}|{_state.PrivateChatUser}|{text}");
            }
            else if (ChatTitle.StartsWith("Комната:"))
            {
                var roomName = ChatTitle.Replace("Комната: ", "");
                var currentRoom = AvailableRooms.FirstOrDefault(r => r.RoomName == roomName);
                if (currentRoom != null)
                {
                    await _network.SendAsync($"ROOM_MESSAGE|{_state.CurrentUser}|{currentRoom.RoomId}|{text}");
                }
                else
                {
                    await _network.SendAsync($"MESSAGE|{_state.CurrentUser}|{text}");
                }
            }
            else
            {
                await _network.SendAsync($"MESSAGE|{_state.CurrentUser}|{text}");
            }
            MessageText = "";
            UpdateDisplayedMessages();
        }

        private void TogglePrivateChat(object param)
        {
            if (SelectedUser == null) return;

            if (_state.CurrentChatMode == "private" && _state.PrivateChatUser == SelectedUser.UserName)
            {
                _state.CurrentChatMode = "public";
                _state.PrivateChatUser = "";

                if (ChatTitle.StartsWith("Приватный чат:"))
                {
                    ChatTitle = _state.PreviousChatTitle ?? "Общий чат";
                }
            }
            else
            {
                _state.CurrentChatMode = "private";
                _state.PrivateChatUser = SelectedUser.UserName;
                _state.PreviousChatTitle = ChatTitle;
                ChatTitle = $"Приватный чат: {SelectedUser.UserName}";
            }

            OnPropertyChanged(nameof(ChatTitle));
            UpdateDisplayedMessages();
        }

        private async void OnMessageReceived(string rawMessage)
        {
            var parts = rawMessage.Split('|');
            if (parts.Length < 1) return;

            Application.Current.Dispatcher.Invoke(() =>
            {
                switch (parts[0])
                {
                    case "LOGIN_SUCCESS":
                    case "REGISTER_SUCCESS":
                        MessageBox.Show("Успешно!", "Успех", MessageBoxButton.OK, MessageBoxImage.Information);
                        LoginText = "";
                        PasswordText = "";
                        UpdateAllProperties();
                        break;

                    case "LOGIN_FAILED":
                    case "REGISTER_FAILED":
                        MessageBox.Show("Ошибка входа/регистрации", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                        _state.CurrentUser = null;
                        UpdateAllProperties();
                        break;
                    case "ROOM_JOINED":
                        if (parts.Length >= 3)
                        {
                            var roomId = parts[1];
                            var roomName = parts[2];

                            ChatTitle = $"Комната: {roomName}";
                            OnPropertyChanged(nameof(ChatTitle));

                            var messagesToRemove = _state.AllMessages
                                .Where(m => !m.IsPrivate && !string.IsNullOrEmpty(m.RoomId) && m.RoomId != roomId)
                                .ToList();

                            foreach (var msg in messagesToRemove)
                            {
                                _state.AllMessages.Remove(msg);
                            }

                            UpdateDisplayedMessages();

                            MessageBox.Show($"Вы вошли в комнату '{roomName}'!", "Успех", MessageBoxButton.OK, MessageBoxImage.Information);
                        }
                        break;

                    case "ROOM_LEFT":
                        
                        if (_state.CurrentChatMode != "private")
                        {
                            ChatTitle = "Общий чат";
                            _state.PreviousChatTitle = "Общий чат";
                            OnPropertyChanged(nameof(ChatTitle));
                        }

                        var roomMessagesToRemove = _state.AllMessages
                            .Where(m => !m.IsPrivate && !string.IsNullOrEmpty(m.RoomId))
                            .ToList();

                        foreach (var msg in roomMessagesToRemove)
                        {
                            _state.AllMessages.Remove(msg);
                        }

                        UpdateDisplayedMessages();
                        MessageBox.Show("Вы вышли из комнаты", "Информация", MessageBoxButton.OK, MessageBoxImage.Information);
                        break;

                    case "ROOM_MESSAGE":
                        if (parts.Length >= 6)
                        {
                            var sender = parts[1];
                            var roomId = parts[2];
                            var roomName = parts[3];
                            var timestamp = parts[4];
                            var messageText = parts[5];

                            var msg = _processor.CreateRoomMessage(sender, roomId, messageText, roomName);
                            _state.AllMessages.Add(msg);
                            UpdateDisplayedMessages();
                        }
                        break;
                    case "ROOM_LIST":
                        if (parts.Length >= 2)
                        {
                            AvailableRooms.Clear();
                            var roomsData = parts[1].Split(';');
                            foreach (var roomInfo in roomsData)
                            {
                                var roomParts = roomInfo.Split(':');
                                if (roomParts.Length >= 3)
                                {
                                    AvailableRooms.Add(new Room
                                    {
                                        RoomId = roomParts[0],
                                        RoomName = roomParts[1],
                                        UserCount = int.Parse(roomParts[2])
                                    });
                                }
                            }
                            OnPropertyChanged(nameof(AvailableRooms));
                        }
                        break;

                    case "NEW_MESSAGE":
                        if (parts.Length >= 4)
                        {
                            var msg = _processor.CreatePublicMessage(parts[1], parts[3]);
                            _state.AllMessages.Add(msg);
                            UpdateDisplayedMessages();
                        }
                        break;

                    case "PRIVATE_MESSAGE":
                        if (parts.Length >= 5)
                        {
                            var msg = _processor.CreatePrivateMessage(parts[1], parts[2], parts[4], _state.CurrentUser);
                            _state.AllMessages.Add(msg);
                            UpdateDisplayedMessages();
                        }
                        break;

                    case "ONLINE_USERS":
                        if (parts.Length >= 2)
                        {
                            _state.OnlineUsers.Clear();
                            foreach (var u in parts[1].Split(',').Where(x => !string.IsNullOrEmpty(x)))
                                _state.OnlineUsers.Add(new User { UserName = u });

                            OnPropertyChanged(nameof(OnlineUsers));
                            OnPropertyChanged(nameof(CanStartPrivateChat));
                            ((RelayCommand)PrivateChatCommand).RaiseCanExecuteChanged();
                        }
                        break;
                }
            });
        }

        private void UpdateDisplayedMessages()
        {
            Application.Current.Dispatcher.Invoke(() =>
            {
                DisplayedMessages.Clear();

                if (_state.AllMessages == null) return;

                IEnumerable<ChatMessage> messages;

                if (_state.CurrentChatMode == "private")
                { 
                    messages = _state.AllMessages.Where(m => m.IsPrivate &&
                        ((m.Sender == _state.CurrentUser && m.Receiver == _state.PrivateChatUser) ||
                         (m.Sender == _state.PrivateChatUser && m.Receiver == _state.CurrentUser)));
                }
                else if (ChatTitle.StartsWith("Комната:"))
                {
                    var currentRoomName = ChatTitle.Replace("Комната: ", "");
                    messages = _state.AllMessages.Where(m =>
                        m.RoomName == currentRoomName ||
                        (!string.IsNullOrEmpty(m.RoomId) && AvailableRooms.Any(r => r.RoomName == currentRoomName && r.RoomId == m.RoomId)));
                }
                else
                {
                    messages = _state.AllMessages.Where(m => !m.IsPrivate && string.IsNullOrEmpty(m.RoomId));
                }

                foreach (var msg in messages.OrderBy(m => m.Timestamp))
                {
                    DisplayedMessages.Add(msg);
                }

                OnPropertyChanged(nameof(DisplayedMessages));
            });
        }

        private void UpdateAllProperties()
        {
            OnPropertyChanged(nameof(IsConnected));
            OnPropertyChanged(nameof(CanSendMessage));
            OnPropertyChanged(nameof(CanConnect));
            OnPropertyChanged(nameof(CanLogin));
            OnPropertyChanged(nameof(CanCreateRoom));
            OnPropertyChanged(nameof(CanRegister));
            OnPropertyChanged(nameof(CanStartPrivateChat));
            OnPropertyChanged(nameof(IsAuthenticated));
            ((RelayCommand)SendMessageCommand).RaiseCanExecuteChanged();
            ((RelayCommand)CreateRoomCommand).RaiseCanExecuteChanged();
            ((RelayCommand)RegisterCommand).RaiseCanExecuteChanged();
            ((RelayCommand)PrivateChatCommand).RaiseCanExecuteChanged();
            ((RelayCommand)ConnectCommand).RaiseCanExecuteChanged();
            ((RelayCommand)LoginCommand).RaiseCanExecuteChanged();
        }

        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged([CallerMemberName] string name = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        }
    }
}

