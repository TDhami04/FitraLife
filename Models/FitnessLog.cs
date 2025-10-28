using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.AspNetCore.Mvc.ModelBinding;

namespace FitraLife.Models
{
    public class FitnessLog
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }

        [BindNever]
        public string UserId { get; set; } = string.Empty;

        [ForeignKey(nameof(UserId))]
        public ApplicationUser? User { get; set; }

        [DataType(DataType.Date)]
        public DateTime Date { get; set; } = DateTime.Today;

        [Display(Name = "Steps Walked")]
        public int Steps { get; set; }

        [Display(Name = "Calories Burned")]
        public double CaloriesBurned { get; set; }

        [Display(Name = "Calories Eaten")]
        public double CaloriesEaten { get; set; }

        [Display(Name = "Workout Time (min)")]
        public int WorkoutMinutes { get; set; }
    }
}
