using System.Threading.Tasks;
using FitraLife.Data;
using FitraLife.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;

namespace FitraLife.Pages.FitnessLogs
{
    [Authorize]
    public class EditModel : PageModel
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;

        public EditModel(ApplicationDbContext context, UserManager<ApplicationUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        [BindProperty]
        public FitnessLog Input { get; set; } = new();

        [TempData]
        public string? StatusMessage { get; set; }

        public async Task<IActionResult> OnGetAsync(int id)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null) return RedirectToPage("/Account/Login");

            var log = await _context.FitnessLogs
                .FirstOrDefaultAsync(f => f.Id == id && f.UserId == user.Id);

            if (log == null)
                return NotFound();

            Input = log;
            return Page();
        }

        public async Task<IActionResult> OnPostAsync()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null) return RedirectToPage("/Account/Login");

            var existingLog = await _context.FitnessLogs
                .FirstOrDefaultAsync(f => f.Id == Input.Id && f.UserId == user.Id);

            if (existingLog == null)
                return NotFound();

            if (!ModelState.IsValid)
                return Page();

            existingLog.Date = Input.Date;
            existingLog.Steps = Input.Steps;
            existingLog.CaloriesBurned = Input.CaloriesBurned;
            existingLog.CaloriesEaten = Input.CaloriesEaten;
            existingLog.WorkoutMinutes = Input.WorkoutMinutes;

            await _context.SaveChangesAsync();

            TempData["StatusMessage"] = "✅ Fitness log updated successfully!";
            return RedirectToPage("/Dashboard/Index");
        }
    }
}
