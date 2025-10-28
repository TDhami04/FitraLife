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

        public IndexModel(UserManager<ApplicationUser> userManager)
        {
            _userManager = userManager;
        }

        [BindProperty]
        public InputModel Input { get; set; } = new();

        [TempData]
        public string? StatusMessage { get; set; }

        public class InputModel
        {
            [EmailAddress]
            public string Email { get; set; } = string.Empty;

            [Range(50, 250, ErrorMessage = "Please enter a valid height (cm).")]
            public double? Height { get; set; }

            [Range(20, 250, ErrorMessage = "Please enter a valid weight (kg).")]
            public double? Weight { get; set; }

            public double? BMI { get; set; }

            [Display(Name = "Fitness Goal")]
            public string? FitnessGoal { get; set; }
        }

        public async Task<IActionResult> OnGetAsync()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
                return RedirectToPage("/Account/Login");

            Input = new InputModel
            {
                Email = user.Email ?? string.Empty,
                Height = user.Height,
                Weight = user.Weight,
                BMI = user.BMI,
                FitnessGoal = user.FitnessGoal
            };

            return Page();
        }

        public async Task<IActionResult> OnPostAsync()
        {
            if (!ModelState.IsValid)
                return Page();

            var user = await _userManager.GetUserAsync(User);
            if (user == null)
                return RedirectToPage("/Account/Login");

            user.Height = Input.Height ?? user.Height;
            user.Weight = Input.Weight ?? user.Weight;
            user.FitnessGoal = Input.FitnessGoal ?? user.FitnessGoal;

            // Recalculate BMI
            if (user.Height > 0 && user.Weight > 0)
            {
                double heightMeters = user.Height / 100.0;
                user.BMI = Math.Round(user.Weight / (heightMeters * heightMeters), 2);
            }

            var result = await _userManager.UpdateAsync(user);

            StatusMessage = result.Succeeded
                ? "Profile updated successfully!"
                : "Error updating profile.";

            return RedirectToPage();
        }
    }
}
