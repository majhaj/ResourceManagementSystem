// <copyright file="Resource.cs" company="PlaceholderCompany">
// Copyright (c) PlaceholderCompany. All rights reserved.
// </copyright>

namespace ResourceManagementSystem.API.Models
{
    public class Resource
    {
        public int Id { get; set; }

        public string Name { get; set; } = string.Empty;

        public string Type { get; set; } = string.Empty;

        public string? Description { get; set; }

        public bool IsAvailable { get; set; } = true;
    }
}
