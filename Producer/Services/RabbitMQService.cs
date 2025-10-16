using RabbitMQ.Client;
using System.Text;
using System.Text.Json;

namespace Producer.Services
{
    public class RabbitMQService
    {
        private IChannel? _channel;

        public async Task InitializeAsync()
        {
            var factory = new ConnectionFactory { HostName = "localhost" };
            var connection = await factory.CreateConnectionAsync();
            _channel = await connection.CreateChannelAsync();

            await _channel.QueueDeclareAsync(queue: "order",
                                             durable: false,
                                             exclusive: false,
                                             autoDelete: false,
                                             arguments: null);
        }

        public async Task SendOrderMessageAsync(object message)
        {
            var jsonMessage = JsonSerializer.Serialize(message, new JsonSerializerOptions
            {
                WriteIndented = false
            });

            var body = Encoding.UTF8.GetBytes(jsonMessage);

            await _channel.BasicPublishAsync(exchange: "",
                                             routingKey: "order",
                                             body: body);
        }
    }
}