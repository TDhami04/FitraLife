namespace FitraLife.Models
{
    public class SavedMealItem
    {
        public int Id { get; set; }
        public string Type { get; set; } = string.Empty; // Breakfast, Lunch, etc.
        public string Name { get; set; } = string.Empty;
        public int Calories { get; set; }
        public string Description { get; set; } = string.Empty;

        public int SavedMealPlanId { get; set; }
        public SavedMealPlan? SavedMealPlan { get; set; }
    }
}
