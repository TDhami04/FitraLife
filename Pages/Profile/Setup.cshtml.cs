using FitraLife.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.ComponentModel.DataAnnotations;
using System.Threading.Tasks;

namespace FitraLife.Pages.Profile
{
    public class SetupModel : PageModel
    {
        private readonly UserManager<ApplicationUser> _userManager;

        public SetupModel(UserManager<ApplicationUser> userManager)
        {
            _userManager = userManager;
        }

        [BindProperty]
        public ApplicationUser Input { get; set; } = new();


        public void OnGet() { }

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

            await _userManager.UpdateAsync(user);

            return RedirectToPage("/Dashboard/Index");
        }
    }
}
