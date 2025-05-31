using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using Moq;
using ResourceManagementSystem.API.Controllers;
using ResourceManagementSystem.API.Data;
using ResourceManagementSystem.API.Hubs;
using ResourceManagementSystem.API.Messaging;
using ResourceManagementSystem.API.Models;
using System;
using System.Collections.Generic;
using System.Security.Claims;
using System.Threading.Tasks;
using Xunit;

namespace ResourceManagementSystem.Tests
{
    public class TasksControllerTests
    {
        private ApplicationDbContext GetDbContext()
        {
            var options = new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseInMemoryDatabase(Guid.NewGuid().ToString())
                .Options;
            return new ApplicationDbContext(options);
        }

        private TasksController GetController(ApplicationDbContext context)
        {
            var publisherMock = new Mock<IMessagePublisher>();
            var hubContextMock = new Mock<IHubContext<TaskSyncHub>>();

            return new TasksController(context, publisherMock.Object, hubContextMock.Object);
        }

        [Fact]
        public async Task GetTasks_ReturnsAllTasks()
        {
            var context = GetDbContext();
            context.Tasks.AddRange(
                new TaskItem { Title = "Task 1" },
                new TaskItem { Title = "Task 2" });
            context.SaveChanges();

            var controller = GetController(context);

            var result = await controller.GetTasks();
            var okResult = Assert.IsType<OkObjectResult>(result.Result);
            var tasks = Assert.IsAssignableFrom<IEnumerable<TaskItem>>(okResult.Value);
            Assert.Collection(tasks,
                t => Assert.Equal("Task 1", t.Title),
                t => Assert.Equal("Task 2", t.Title));
        }

        [Fact]
        public async Task GetTask_ReturnsTask_WhenExists()
        {
            var context = GetDbContext();
            var task = new TaskItem { Title = "Sample Task" };
            context.Tasks.Add(task);
            context.SaveChanges();

            var controller = GetController(context);

            var result = await controller.GetTask(task.Id);
            var okResult = Assert.IsType<OkObjectResult>(result.Result);
            var returnedTask = Assert.IsType<TaskItem>(okResult.Value);
            Assert.Equal("Sample Task", returnedTask.Title);
        }

        [Fact]
        public async Task CreateTask_AddsTaskAndReturnsCreated()
        {
            var context = GetDbContext();
            var controller = GetController(context);
            var task = new TaskItem { Title = "New Task" };

            var result = await controller.CreateTask(task);
            var createdAt = Assert.IsType<CreatedAtActionResult>(result.Result);
            var created = Assert.IsType<TaskItem>(createdAt.Value);

            Assert.Equal("New Task", created.Title);
        }

        [Fact]
        public async Task UpdateTask_ChangesTask_WhenIdMatches()
        {
            var context = GetDbContext();
            var task = new TaskItem { Title = "Original" };
            context.Tasks.Add(task);
            context.SaveChanges();

            var controller = GetController(context);
            task.Title = "Updated";

            var result = await controller.UpdateTask(task.Id, task);
            Assert.IsType<NoContentResult>(result);

            var updated = await context.Tasks.FindAsync(task.Id);
            Assert.Equal("Updated", updated.Title);
        }

        [Fact]
        public async Task DeleteTask_RemovesTask_WhenExists()
        {
            var context = GetDbContext();
            var task = new TaskItem { Title = "To Delete" };
            context.Tasks.Add(task);
            context.SaveChanges();

            var controller = GetController(context);
            var result = await controller.DeleteTask(task.Id);

            Assert.IsType<NoContentResult>(result);
            Assert.Null(await context.Tasks.FindAsync(task.Id));
        }

        [Fact]
        public async Task GetTasksForUser_ReturnsOnlyAssignedTasks()
        {
            var context = GetDbContext();
            var user = new User { Username = "U1" };
            context.Users.Add(user);
            context.SaveChanges();

            context.Tasks.AddRange(
                new TaskItem { Title = "Assigned", AssignedToUserId = user.Id },
                new TaskItem { Title = "Unassigned" });
            context.SaveChanges();

            var controller = GetController(context);
            var result = await controller.GetTasksForUser(user.Id);

            var okResult = Assert.IsType<OkObjectResult>(result.Result);
            var tasks = Assert.IsAssignableFrom<List<TaskItem>>(okResult.Value);
            Assert.Single(tasks);
            Assert.Equal("Assigned", tasks[0].Title);
        }

        [Fact]
        public async Task GetTasksForCurrentUser_ReturnsUserTasks_WhenAuthenticated()
        {
            var context = GetDbContext();
            var user = new User { Username = "me" };
            context.Users.Add(user);
            context.Tasks.Add(new TaskItem { Title = "My Task", AssignedToUserId = user.Id });
            context.SaveChanges();

            var controller = GetController(context);
            controller.ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext
                {
                    User = new ClaimsPrincipal(new ClaimsIdentity(new[]
                    {
                        new Claim("sub", user.Id.ToString())
                    }))
                }
            };

            var result = await controller.GetTasksForCurrentUser();
            var okResult = Assert.IsType<OkObjectResult>(result.Result);
            var tasks = Assert.IsAssignableFrom<List<TaskItem>>(okResult.Value);
            Assert.Single(tasks);
            Assert.Equal("My Task", tasks[0].Title);
        }
    }
}
