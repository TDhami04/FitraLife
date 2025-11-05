using System;
using System.Collections.Generic;

namespace FitraLife.Models
{
    public class WorkoutPlan
    {
        public int Id { get; set; }
        public string Title { get; set; } = "Workout Plan";
        public string Goal { get; set; } = string.Empty;
        public string CreatedById { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public string Notes { get; set; } = string.Empty;

        public ICollection<Exercise> Exercises { get; set; } = new List<Exercise>();
    }
}
