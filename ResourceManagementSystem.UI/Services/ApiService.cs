using System.Net.Http;
using System.Net.Http.Json;
using ResourceManagementSystem.API.Models;

public class ApiService
{
    private readonly HttpClient _http;

    public ApiService(HttpClient http)
    {
        _http = http;
    }

    public async Task<List<TaskItem>> GetTasksAsync()
        => await _http.GetFromJsonAsync<List<TaskItem>>("api/tasks");

    public async Task CreateTaskAsync(TaskItem task)
        => await _http.PostAsJsonAsync("api/tasks", task);
}
