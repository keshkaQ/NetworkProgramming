using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System.Text;
using System.Text.Json;

namespace Consumer.Services
{
    public class QueueService
    {
        private IConnection? _connection;
        private IChannel? _channel;
        private bool _isListening;

        public event Action<string>? MessageReceived;
        public event Action<string>? StatusChanged;

        public async Task StartListeningAsync()
        {
            try
            {
                if (_isListening)
                { 
                    OnStatusChanged("Уже слушаем очередь");
                    return;
                }
                var factory = new ConnectionFactory { HostName = "localhost" };
                _connection = await factory.CreateConnectionAsync();
                _channel = await _connection.CreateChannelAsync();

                await _channel.QueueDeclareAsync(
                    queue: "order",
                    durable: false,
                    exclusive: false,
                    autoDelete: false,
                    arguments: null);

                OnStatusChanged("Очередь создана");

                var consumer = new AsyncEventingBasicConsumer(_channel);
                consumer.ReceivedAsync += async (model, ea) =>
                {
                    try
                    {
                        var body = ea.Body.ToArray();
                        var message = Encoding.UTF8.GetString(body);
                        LogShortMessageInfo(message);
                    }
                    catch (Exception ex)
                    {
                        OnStatusChanged($"Ошибка обработки сообщения: {ex.Message}");
                    }
                  
                };

                await _channel.BasicConsumeAsync(
                    queue: "order",
                    autoAck: true,
                    consumer: consumer);

                _isListening = true;
                OnStatusChanged("Consumer слушает очередь...");
            }
            catch (Exception ex)
            {
                _isListening = false;
                throw;
            }
        }

        private void LogShortMessageInfo(string message)
        {
            try
            {
                using var orderData = JsonDocument.Parse(message);
                var root = orderData.RootElement;
                var modelName = GetPropertyValue(root, "ModelName");
                var customerName = GetPropertyValue(root, "CustomerName");
                var color = GetPropertyValue(root, "Color");
                var storage = GetPropertyValue(root, "Storage");
                var simType = GetPropertyValue(root, "SimType");
                var price = GetPropertyValue(root, "FinalPrice");

                if (price != "Не указано")
                    price = $"{price} ₽";

                var shortInfo = $"📦 Новый заказ: {modelName} | 👤 {customerName} | 🎨 {color} | 💾 {storage} | 💰 {price}";
                OnStatusChanged(shortInfo);
            }
            catch (Exception ex)
            {
                OnStatusChanged($"❌ Ошибка парсинга: {ex.Message}");
            }
        }

        private string GetPropertyValue(JsonElement element, string propertyName, string defaultValue = "Не указано")
        {
            if (!element.TryGetProperty(propertyName, out var value) || value.ValueKind == JsonValueKind.Null)
                return defaultValue;

            try
            {
                return value.ValueKind switch
                {
                    JsonValueKind.String => value.GetString() ?? defaultValue,
                    JsonValueKind.Number => value.GetDecimal().ToString("N0"), 
                    _ => value.ToString()
                };
            }
            catch
            {
                return defaultValue;
            }
        }

        public async Task StopListeningAsync()
        {
            try
            {
                if (_channel?.IsOpen == true)
                {
                    await _channel.CloseAsync();
                }
                _channel?.Dispose();
                _channel = null;

                if (_connection?.IsOpen == true)
                {
                    await _connection.CloseAsync();
                }
                _connection?.Dispose();
                _connection = null;

                _isListening = false;
                OnStatusChanged("Очередь остановлена");
            }
            catch (Exception ex)
            {
                OnStatusChanged($"Ошибка при остановке: {ex.Message}");
                throw;
            }
        }

        protected virtual void OnStatusChanged(string status)
        {
            StatusChanged?.Invoke(status);
        }
    }
}