using FitraLife.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.ComponentModel.DataAnnotations;
using System.Threading.Tasks;

namespace FitraLife.Pages.Profile
{
    public class IndexModel : PageModel
    {
        private readonly UserManager<ApplicationUser> _userManager;
        
        public bool HasProfileComplete { get; set; }
        public bool HasActiveGoal { get; set; }
        public bool HasBMIRecorded { get; set; }
        public int ProfileCompleteness { get; set; }

        public IndexModel(UserManager<ApplicationUser> userManager)
        {
            _userManager = userManager;
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

            var result = await _userManager.UpdateAsync(user);

            StatusMessage = result.Succeeded
                ? "Profile updated successfully!"
                : "Error updating profile.";

            return RedirectToPage();
        }
    }
}
