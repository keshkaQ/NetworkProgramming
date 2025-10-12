using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Text;

namespace ClientChatApp.Services
{
    public class NetworkService
    {
        private TcpClient? _client;
        private NetworkStream? _stream;
        public bool IsConnected => _client?.Connected == true;

        public event Action<string>? OnMessageReceived;
        public event Action<string>? OnError;

        public async Task ConnectAsync(string ip, int port)
        {
            try
            {
                _client = new TcpClient();
                await _client.ConnectAsync(IPAddress.Parse(ip), port);
                _stream = _client.GetStream();
                _ = Task.Run(ReceiveMessagesAsync);
            }
            catch (Exception ex)
            {
                OnError?.Invoke($"❌ Ошибка подключения: {ex.Message}");
            }
        }

        public async Task SendAsync(string message)
        {
            try
            {
                if (_client?.Connected == false)
                {
                    OnError?.Invoke("Нет подключения к серверу");
                    return;
                }
                var data = Encoding.UTF8.GetBytes(message + "\n");
                await _stream.WriteAsync(data, 0, data.Length);
            }
            catch (Exception ex)
            {
                OnError?.Invoke($"Ошибка отправки: {ex.Message}");
            }
        }

        private async Task ReceiveMessagesAsync()
        {
            var reader = new StreamReader(_stream, Encoding.UTF8);
            while (_client?.Connected == true)
            {
                try
                {
                    var message = await reader.ReadLineAsync();
                    if (message == null) break;
                    if (!string.IsNullOrEmpty(message))
                    {
                        OnMessageReceived?.Invoke(message);
                    }
                }
                catch (Exception ex)
                {
                    OnError?.Invoke($"Ошибка получения: {ex.Message}");
                    break;
                }
            }
        }

        public void Disconnect()
        {
            _stream?.Close();
            _client?.Close();
            _stream = null;
            _client = null;
        }
    }
}
