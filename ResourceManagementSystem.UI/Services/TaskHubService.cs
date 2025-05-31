using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.SignalR.Client;
using ResourceManagementSystem.API.Models;

public class TaskHubService
{
    private readonly HubConnection _connection;

    public TaskHubService(NavigationManager nav)
    {
        _connection = new HubConnectionBuilder()
            .WithUrl(nav.ToAbsoluteUri("/tasksynchub"))
            .WithAutomaticReconnect()
            .Build();
    }

    public async Task ConnectAsync(Action<TaskItem> onUpdate)
    {
        _connection.On<TaskItem>("TaskUpdated", onUpdate);
        await _connection.StartAsync();
    }

    public async Task DisconnectAsync()
    {
        if (_connection.State == HubConnectionState.Connected)
        {
            await _connection.StopAsync();
        }
    }
}
