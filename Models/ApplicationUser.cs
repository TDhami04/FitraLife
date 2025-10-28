using Microsoft.AspNetCore.Identity;
using System.ComponentModel.DataAnnotations;

namespace FitraLife.Models
{
    public class ApplicationUser : IdentityUser
    {

        /*// Fitness tracking fields
        public double Height { get; set; }   // in cm
        public double Weight { get; set; }   // in kg
        public string FitnessGoal { get; set; } = string.Empty;

        [StringLength(30)]
        public string ActivityLevel { get; set; } = string.Empty;

        // Computed field - not stored directly, but useful in UI
        public double BMI => (Height > 0) ? Weight / Math.Pow(Height / 100, 2) : 0;

        public DateTime CreatedAt { get; set; } = DateTime.Now;*/
        [Range(1, 120)]
        public int Age { get; set; }
         public double Height { get; set; }   // in cm or meters (decide convention)
        public double Weight { get; set; }   // in kg
        public double BMI { get; set; }      // computed/stored
        public string FitnessGoal { get; set; } = string.Empty;

    }
}
