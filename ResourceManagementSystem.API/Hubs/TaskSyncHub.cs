using Microsoft.AspNetCore.SignalR;

namespace ResourceManagementSystem.API.Hubs
{
    public class TaskSyncHub : Hub
    {
        public async Task NotifyTaskUpdated(object task)
        {
            await Clients.Others.SendAsync("TaskUpdated", task);
        }
    }
}
