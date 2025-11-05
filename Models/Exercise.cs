namespace FitraLife.Models
{
    public class Exercise
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public string MuscleGroup { get; set; } = string.Empty;
        public int? Reps { get; set; }
        public int? Sets { get; set; }
        public int? Minutes { get; set; }
        public int Order { get; set; } = 0;

        public int WorkoutPlanId { get; set; }
        public WorkoutPlan? WorkoutPlan { get; set; }
    }
}
