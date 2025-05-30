using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System.Text;
using System.Text.Json;
using ResourceManagementSystem.API.Models;

namespace ResourceManagementSystem.Sync
{
    public class TaskUpdateConsumer
    {
        private readonly IConnection _connection;
        private readonly IModel _channel;

        public TaskUpdateConsumer()
        {
            var factory = new ConnectionFactory() { HostName = "localhost" };
            _connection = factory.CreateConnection();
            _channel = _connection.CreateModel();

            _channel.QueueDeclare(queue: "task-updates",
                                  durable: false,
                                  exclusive: false,
                                  autoDelete: false,
                                  arguments: null);
        }

        public void StartConsuming()
        {
            var consumer = new EventingBasicConsumer(_channel);

            consumer.Received += (model, ea) =>
            {
                var body = ea.Body.ToArray();
                var message = Encoding.UTF8.GetString(body);

                var task = JsonSerializer.Deserialize<TaskItem>(message);

                Console.WriteLine($"[SYNC] Odebrano zadanie: {task?.Title}");
            };

            _channel.BasicConsume(queue: "task-updates",
                                  autoAck: true,
                                  consumer: consumer);

            Console.WriteLine("Nasłuchiwanie wiadomości z RabbitMQ...");
        }
    }
}
