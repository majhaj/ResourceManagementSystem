using System.Text;
using System.Text.Json;
using RabbitMQ.Client;
using ResourceManagementSystem.API.Models;

namespace ResourceManagementSystem.API.Messaging
{
    public class RabbitMqPublisher : IMessagePublisher
    {
        private readonly IConnection? _connection;
        private readonly IModel? _channel;
        private readonly bool _isConnected = false;

        public RabbitMqPublisher()
        {
            try
            {
                var factory = new ConnectionFactory
                {
                    HostName = "localhost",
                    UserName = "guest", // domyślne dla RabbitMQ
                    Password = "guest"
                };

                _connection = factory.CreateConnection();
                _channel = _connection.CreateModel();

                _channel.QueueDeclare(
                    queue: "task-updates",
                    durable: false,
                    exclusive: false,
                    autoDelete: false,
                    arguments: null
                );

                _isConnected = true;
                Console.WriteLine("[RabbitMQ] Połączono z brokerem.");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[RabbitMQ] Błąd połączenia: {ex.Message}");
            }
        }

        /// <inheritdoc/>
        public void PublishTaskUpdate(TaskItem task)
        {
            if (!_isConnected || _channel == null)
            {
                Console.WriteLine("[RabbitMQ] Nie połączono – pomijam publikację wiadomości.");
                return;
            }

            var message = JsonSerializer.Serialize(task);
            var body = Encoding.UTF8.GetBytes(message);

            _channel.BasicPublish(
                exchange: "",
                routingKey: "task-updates",
                basicProperties: null,
                body: body
            );

            Console.WriteLine($"[RabbitMQ] Wiadomość opublikowana: {message}");
        }
    }
}
