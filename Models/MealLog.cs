using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace FitraLife.Models
{
    public class MealLog
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }

        [Required]
        public string UserId { get; set; } = string.Empty;

        [ForeignKey(nameof(UserId))]
        public ApplicationUser? User { get; set; }

        [DataType(DataType.Date)]
        public DateTime Date { get; set; } = DateTime.Today;

        [Required]
        public string MealType { get; set; } = string.Empty; // Breakfast, Lunch, Dinner, Snack

        [Required]
        public string Name { get; set; } = string.Empty;

        public int Calories { get; set; }

        public string? Description { get; set; }

        public string? Protein { get; set; }
        public string? Carbs { get; set; }
        public string? Fats { get; set; }
    }
}
