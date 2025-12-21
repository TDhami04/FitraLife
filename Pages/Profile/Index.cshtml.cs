using FitraLife.Data;
using FitraLife.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace FitraLife.Pages.Profile
{
    public class IndexModel : PageModel
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly ApplicationDbContext _context;
        
        public bool HasProfileComplete { get; set; }
        public bool HasActiveGoal { get; set; }
        public bool HasBMIRecorded { get; set; }
        public int ProfileCompleteness { get; set; }

        public List<WorkoutPlan> SavedWorkouts { get; set; } = new();
        public List<SavedMealPlan> SavedMeals { get; set; } = new();

        public IndexModel(UserManager<ApplicationUser> userManager, ApplicationDbContext context)
        {
            _userManager = userManager;
            _context = context;
        }

        [BindProperty]
        public ApplicationUser Input { get; set; } = new();

        [TempData]
        public string? StatusMessage { get; set; }

        public async Task<IActionResult> OnGetAsync()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
                return RedirectToPage("/Account/Login");

            Input = user;

            // Fetch Saved Plans
            SavedWorkouts = await _context.WorkoutPlans
                .Include(p => p.Exercises)
                .Where(p => p.CreatedById == user.Id)
                .OrderByDescending(p => p.CreatedAt)
                .ToListAsync();

            SavedMeals = await _context.SavedMealPlans
                .Include(p => p.Meals)
                .Where(p => p.CreatedById == user.Id)
                .OrderByDescending(p => p.CreatedAt)
                .ToListAsync();

            HasBMIRecorded = Input.BMI > 0;
            HasActiveGoal = (Input.StepGoal > 0) || (Input.WorkoutMinutesGoal > 0) || !string.IsNullOrEmpty(Input.FitnessGoal);

            int points = 0;
            if (Input.Age > 0) points++;
            if (!string.IsNullOrEmpty(Input.Gender)) points++;
            if (Input.Height > 0) points++;
            if (Input.Weight > 0) points++;
            if (!string.IsNullOrEmpty(Input.ActivityLevel)) points++;
            if (!string.IsNullOrEmpty(Input.GoalType)) points++;
            ProfileCompleteness = (int)Math.Round((points / 6.0) * 100);
            HasProfileComplete = ProfileCompleteness >= 80; 

            return Page();
        }

        public async Task<IActionResult> OnPostAsync()
        {
            if (!ModelState.IsValid)
                return Page();

            var user = await _userManager.GetUserAsync(User);
            if (user == null)
                return RedirectToPage("/Account/Login");

            user.Height = Input.Height;
            user.Weight = Input.Weight;
            user.Age = Input.Age;
            user.Gender = Input.Gender;
            user.ActivityLevel = Input.ActivityLevel;
            user.GoalType = Input.GoalType;
            user.FitnessGoal = Input.FitnessGoal;
            user.BMI = Math.Round(user.Weight / Math.Pow(user.Height / 100, 2), 1);
            user.StepGoal = Input.StepGoal;
            user.WorkoutMinutesGoal = Input.WorkoutMinutesGoal;
            user.TargetWeight = Input.TargetWeight;

            var result = await _userManager.UpdateAsync(user);

            StatusMessage = result.Succeeded
                ? "Profile updated successfully!"
                : "Error updating profile.";

            return RedirectToPage();
        }

        public async Task<IActionResult> OnPostDeleteWorkoutAsync(int id)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null) return Challenge();

            var plan = await _context.WorkoutPlans.FindAsync(id);
            if (plan != null && plan.CreatedById == user.Id)
            {
                _context.WorkoutPlans.Remove(plan);
                await _context.SaveChangesAsync();
                StatusMessage = "Workout plan deleted.";
            }
            return RedirectToPage();
        }

        public async Task<IActionResult> OnPostDeleteMealAsync(int id)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null) return Challenge();

            var plan = await _context.SavedMealPlans.FindAsync(id);
            if (plan != null && plan.CreatedById == user.Id)
            {
                _context.SavedMealPlans.Remove(plan);
                await _context.SaveChangesAsync();
                StatusMessage = "Meal plan deleted.";
            }
            return RedirectToPage();
        }
    }
}
