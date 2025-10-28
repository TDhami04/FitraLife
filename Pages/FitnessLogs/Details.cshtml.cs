using System.Threading.Tasks;
using FitraLife.Data;
using FitraLife.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace FitraLife.Pages.FitnessLogs
{
    [Authorize]
    public class DetailsModel : PageModel
    {
        private readonly ApplicationDbContext _context;

        public DetailsModel(ApplicationDbContext context)
        {
            _context = context;
        }

        public FitnessLog? Log { get; set; }

        public async Task<IActionResult> OnGetAsync(int id)
        {
            Log = await _context.FitnessLogs.FindAsync(id);
            if (Log == null)
                return NotFound();

            return Page();
        }
    }
}
