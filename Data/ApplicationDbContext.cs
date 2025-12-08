using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using FitraLife.Models;

namespace FitraLife.Data
{
    public class ApplicationDbContext : IdentityDbContext<ApplicationUser>
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        public DbSet<FitnessLog> FitnessLogs { get; set; }
        public DbSet<MealLog> MealLogs { get; set; }
        public DbSet<WorkoutPlan> WorkoutPlans { get; set; }
        public DbSet<Exercise> Exercises { get; set; }
        public DbSet<SavedMealPlan> SavedMealPlans { get; set; }
        public DbSet<SavedMealItem> SavedMealItems { get; set; }
    }
}
