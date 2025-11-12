using Microsoft.AspNetCore.Identity;
using System.ComponentModel.DataAnnotations;

namespace FitraLife.Models
{
    public class ApplicationUser : IdentityUser
    {
        [Range(1, 120)]
        public int Age { get; set; }
        public string Gender { get; set; } = "Male";
        public string ActivityLevel { get; set; } = "Moderate";
        public double Height { get; set; }   // in cm 
        public double Weight { get; set; }   // in kg
        public double BMI { get; set; }
        public string FitnessGoal { get; set; } = string.Empty;
        public string GoalType { get; set; } = "Maintain";
        public int StepGoal { get; set; } = 70000;
        public int WorkoutMinutesGoal { get; set; } = 100;

    }
}
