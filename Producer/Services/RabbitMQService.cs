using RabbitMQ.Client;
using System.Text;
using System.Text.Encodings.Web;
using System.Text.Json;
using System.Text.Unicode;
using System.Windows;

namespace Producer.Services
{
    public class RabbitMQService
    {
        private IChannel? _channel;
        private bool _isInitialized = false;

        public async Task<bool> InitializeAsync()
        {
            try
            {
                var factory = new ConnectionFactory { HostName = "localhost" };
                var connection = await factory.CreateConnectionAsync();
                _channel = await connection.CreateChannelAsync();

                await _channel.QueueDeclareAsync(queue: "order",
                                                 durable: false,
                                                 exclusive: false,
                                                 autoDelete: false,
                                                 arguments: null);

                _isInitialized = true;
                return true;
            }
            catch (Exception ex)
            {
                _isInitialized = false;
                throw new Exception($"Ошибка инициализации RabbitMQ: {ex.Message}", ex);
            }
        }

        public async Task<bool> SendOrderMessageAsync(object message)
        {
            if (!_isInitialized || _channel == null)
            {
                throw new InvalidOperationException("RabbitMQ не инициализирован");
            }

            try
            {
                var jsonMessage = JsonSerializer.Serialize(message, new JsonSerializerOptions
                {
                    WriteIndented = false,
                    Encoder = JavaScriptEncoder.Create(UnicodeRanges.BasicLatin, UnicodeRanges.Cyrillic)
                });

                var body = Encoding.UTF8.GetBytes(jsonMessage);

                await _channel.BasicPublishAsync(exchange: "",
                                                 routingKey: "order",
                                                 body: body);
                return true;
            }
            catch (Exception ex)
            {
                throw new Exception($"Ошибка отправки сообщения: {ex.Message}", ex);
            }
        }
    }
}