using ResourceManagementSystem.API.Models;

namespace ResourceManagementSystem.API.Messaging
{
    public interface IMessagePublisher
    {
        void PublishTaskUpdate(TaskItem task);
    }
}
