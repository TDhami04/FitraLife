using System.Threading.Tasks;
using FitraLife.Data;
using FitraLife.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace FitraLife.Pages.FitnessLogs
{
    [Authorize]
    public class CreateModel : PageModel
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;

        public CreateModel(ApplicationDbContext context, UserManager<ApplicationUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        [BindProperty]
        public FitnessLog Input { get; set; } = new();

        [TempData]
        public string? StatusMessage { get; set; }

        public void OnGet()
        {
            Input.Date = DateTime.Today;
        }

        public async Task<IActionResult> OnPostAsync()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null) return RedirectToPage("/Account/Login");

            Input.UserId = user.Id;

            if (!ModelState.IsValid)
                return Page();

            _context.FitnessLogs.Add(Input);
            await _context.SaveChangesAsync();

            return RedirectToPage("/Dashboard/Index");
        }
    }
}
