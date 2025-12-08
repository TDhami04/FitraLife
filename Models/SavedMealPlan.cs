using System;
using System.Collections.Generic;

namespace FitraLife.Models
{
    public class SavedMealPlan
    {
        public int Id { get; set; }
        public string Title { get; set; } = "Meal Plan";
        public string DietType { get; set; } = string.Empty;
        public int TargetCalories { get; set; }
        public int TotalCalories { get; set; }
        public string Protein { get; set; } = "0g";
        public string Carbs { get; set; } = "0g";
        public string Fats { get; set; } = "0g";
        public string CreatedById { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public ICollection<SavedMealItem> Meals { get; set; } = new List<SavedMealItem>();
    }
}
