public class TaskItem
{
        public int Id { get; set; }

        public string Title { get; set; } = string.Empty;

        public string? Description { get; set; }

        public string Status { get; set; } = "ToDo";

        public DateTime? Deadline { get; set; }

        public DateTime CreatedAt { get; set; }

        public DateTime UpdatedAt { get; set; }

        public int? AssignedToUserId { get; set; }

        public UserDto? AssignedToUser { get; set; }
}
