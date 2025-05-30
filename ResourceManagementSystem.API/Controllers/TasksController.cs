using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using ResourceManagementSystem.API.Data;
using ResourceManagementSystem.API.Hubs;
using ResourceManagementSystem.API.Messaging;
using ResourceManagementSystem.API.Models;

[Authorize]
[ApiController]
[Route("api/[controller]")]
public class TasksController : ControllerBase
{
    private readonly ApplicationDbContext _context;
    private readonly IMessagePublisher _publisher;
    private readonly IHubContext<TaskHub> _hubContext;

    public TasksController(ApplicationDbContext context, IMessagePublisher publisher, IHubContext<TaskHub> hubContext)
    {
        _context = context;
        _publisher = publisher;
        _hubContext = hubContext;
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<TaskItem>>> GetTasks()
    {
        return await _context.Tasks.Include(t => t.AssignedToUser).ToListAsync();
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<TaskItem>> GetTask(int id)
    {
        var task = await _context.Tasks
                                 .Include(t => t.AssignedToUser)
                                 .FirstOrDefaultAsync(t => t.Id == id);

        return task is null ? NotFound() : Ok(task);
    }

    [HttpPost]
    public async Task<ActionResult<TaskItem>> CreateTask(TaskItem task)
    {
        task.CreatedAt = DateTime.UtcNow;
        task.UpdatedAt = DateTime.UtcNow;

        _context.Tasks.Add(task);
        await _context.SaveChangesAsync();

        // Publikacja i wysyłka
        _publisher.PublishTaskUpdate(task);
        await _hubContext.Clients.All.SendAsync("TaskUpdated", task);

        return CreatedAtAction(nameof(GetTask), new { id = task.Id }, task);
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> UpdateTask(int id, TaskItem task)
    {
        if (id != task.Id) return BadRequest();

        var existing = await _context.Tasks.FindAsync(id);
        if (existing is null) return NotFound();

        existing.Title = task.Title;
        existing.Description = task.Description;
        existing.Status = task.Status;
        existing.Deadline = task.Deadline;
        existing.AssignedToUserId = task.AssignedToUserId;
        existing.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync();

        _publisher.PublishTaskUpdate(existing);
        await _hubContext.Clients.All.SendAsync("TaskUpdated", existing);

        return NoContent();
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteTask(int id)
    {
        var task = await _context.Tasks.FindAsync(id);
        if (task is null) return NotFound();

        _context.Tasks.Remove(task);
        await _context.SaveChangesAsync();

        _publisher.PublishTaskUpdate(task);
        await _hubContext.Clients.All.SendAsync("TaskDeleted", id);

        return NoContent();
    }

    [HttpGet("/api/users/{userId}/tasks")]
    public async Task<ActionResult<IEnumerable<TaskItem>>> GetTasksForUser(int userId)
    {
        var userExists = await _context.Users.AnyAsync(u => u.Id == userId);
        if (!userExists) return NotFound();

        var tasks = await _context.Tasks
                                  .Where(t => t.AssignedToUserId == userId)
                                  .ToListAsync();

        return tasks;
    }
}
