using ServerChat.Handlers;
using ServerChat.Managers;
using System.ComponentModel;
using System.Net;
using System.Net.Sockets;

namespace ServerChat
{
    public class ChatServer : INotifyPropertyChanged
    {
        private TcpListener _listener;
        private readonly UserManager _userManager;
        private readonly MessageManager _messageManager;
        private RoomManager _roomManager;
        private ClientHandler _clientHandler;
        private bool _isRunning = false;
        public event Action<string> OnLogMessage;

        private int _connectedClientsCount;
        public int ConnectedClientsCount
        {
            get => _connectedClientsCount;
            set
            {
                _connectedClientsCount = value;
                OnPropertyChanged(nameof(ConnectedClientsCount));
            }
        }


        public ChatServer()
        {
            _userManager = new UserManager();
            _messageManager = new MessageManager();
            _roomManager = new RoomManager();
        }

        public void StartServer(int port)
        {
            try
            {
                _clientHandler = new ClientHandler(_userManager, _roomManager, _messageManager, OnLogMessage,UpdateClientsCount);
                _listener = new TcpListener(IPAddress.Any, port);
                _listener.Start();
                _isRunning = true;
                UpdateClientsCount(); 

                OnLogMessage?.Invoke($"✅ Сервер запущен на порту {port}");
                _ = Task.Run(AcceptClientsAsync);
            }
            catch (Exception ex)
            {
                OnLogMessage?.Invoke($"❌ Ошибка запуска сервера: {ex.Message}");
                throw;
            }
        }
        public void UpdateClientsCount()
        {
            ConnectedClientsCount = _userManager._clientUsers.Count;
        }

        public void StopServer()
        {
            _isRunning = false;
            foreach (var client in _userManager._clientUsers.Keys.ToArray())
            {
                client.Close();
            }
            _listener?.Stop();
            UpdateClientsCount();
        }

        private async Task AcceptClientsAsync()
        {
            while (_isRunning)
            {
                try
                {
                    var client = await _listener.AcceptTcpClientAsync();
                    _ = Task.Run(() => _clientHandler.HandleClientAsync(client));
                    ConnectedClientsCount = _userManager._clientUsers.Count + 1;
                    OnLogMessage?.Invoke($"🔗 Новый клиент подключен. Всего клиентов: {ConnectedClientsCount}");
                }
                catch (Exception ex)
                {
                    if (_isRunning)
                        OnLogMessage?.Invoke($"❌ Ошибка принятия подключения: {ex.Message}");
                }
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;
        protected virtual void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}