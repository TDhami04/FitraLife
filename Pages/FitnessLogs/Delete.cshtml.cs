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
    public class DeleteModel : PageModel
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;

        public DeleteModel(ApplicationDbContext context, UserManager<ApplicationUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        [BindProperty]
        public FitnessLog? Log { get; set; }

        public async Task<IActionResult> OnGetAsync(int id)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null) return RedirectToPage("/Account/Login");

            Log = await _context.FitnessLogs
                .FirstOrDefaultAsync(f => f.Id == id && f.UserId == user.Id);

            if (Log == null)
                return NotFound();

            return Page();
        }

        public async Task<IActionResult> OnPostAsync(int id)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null) return RedirectToPage("/Account/Login");

            var log = await _context.FitnessLogs
                .FirstOrDefaultAsync(f => f.Id == id && f.UserId == user.Id);

            if (log == null)
                return NotFound();

            _context.FitnessLogs.Remove(log);
            await _context.SaveChangesAsync();

            TempData["StatusMessage"] = "🗑️ Fitness log deleted successfully!";
            return RedirectToPage("/Dashboard/Index");
        }
    }
}
