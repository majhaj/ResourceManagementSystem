using Microsoft.EntityFrameworkCore.Metadata;
using RabbitMQ.Client;
using ResourceManagementSystem.API.Models;
using System.Text;
using System.Text.Json;

namespace ResourceManagementSystem.API.Services
{
    public class RabbitMqPublisher
    {
        private readonly IModel _channel;

        public RabbitMqPublisher()
        {
            var factory = new ConnectionFactory() { HostName = "localhost" };
            var connection = factory.CreateConnection(new string[] { "localhost" });
            _channel = connection.CreateModel();
            _channel.QueueDeclare(queue: "task_updates", durable: false, exclusive: false, autoDelete: false);
        }

        public void PublishTaskUpdate(TaskItem task)
        {
            var message = JsonSerializer.Serialize(task);
            var body = Encoding.UTF8.GetBytes(message);
            _channel.BasicPublish("amq.direct", "task_updates", null, body);
        }
    }
}
