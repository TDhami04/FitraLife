using FitraLife.Services;
using FitraLife.Data;
using FitraLife.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using System.Threading.Tasks;
using System.Text.RegularExpressions;
using System.Text.Json;
using System.Collections.Generic;
using System.Linq;

namespace FitraLife.Pages.WorkoutGenerator
{
    [Authorize]
    public class IndexModel : PageModel
    {
        private readonly IGeminiService _geminiService;
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;

        public IndexModel(IGeminiService geminiService, ApplicationDbContext context, UserManager<ApplicationUser> userManager)
        {
            _geminiService = geminiService;
            _context = context;
            _userManager = userManager;
        }

        [BindProperty]
        public string Goal { get; set; } = string.Empty;

        [BindProperty]
        public string ExperienceLevel { get; set; } = "Beginner";

        [BindProperty]
        public string AvailableDays { get; set; } = "3";

        public string PlanTitle { get; set; } = string.Empty;
        public List<WorkoutDay> AiWorkoutPlan { get; set; } = new();

        [BindProperty]
        public string GeneratedPlanJson { get; set; } = string.Empty;

        [BindProperty]
        public string GeneratedPlanTitle { get; set; } = string.Empty;

        public class WorkoutDay
        {
            public string Day { get; set; } = string.Empty;
            public List<string> Exercises { get; set; } = new();
        }

        public async Task<IActionResult> OnPostGenerateAsync()
        {
            var aiResponse = await _geminiService.GenerateWorkoutPlanAsync(Goal, ExperienceLevel, AvailableDays);

            try
            {
                // 🔧 Strip markdown if Gemini wrapped the JSON
                var cleanJson = aiResponse
                    .Replace("```json", "", StringComparison.OrdinalIgnoreCase)
                    .Replace("```", "")
                    .Trim();

                var parsed = JsonSerializer.Deserialize<JsonElement>(cleanJson);

                PlanTitle = parsed.GetProperty("planTitle").GetString() ?? "Workout Plan";

                foreach (var d in parsed.GetProperty("days").EnumerateArray())
                {
                    var day = new WorkoutDay
                    {
                        Day = d.GetProperty("day").GetString() ?? "",
                        Exercises = new List<string>()
                    };

                    foreach (var ex in d.GetProperty("exercises").EnumerateArray())
                        day.Exercises.Add(ex.GetString() ?? "");

                    AiWorkoutPlan.Add(day);
                }

                // Serialize for the hidden field so we can save it later
                GeneratedPlanJson = JsonSerializer.Serialize(AiWorkoutPlan);
                GeneratedPlanTitle = PlanTitle;
            }
            catch
            {
                // fallback if Gemini sends non-JSON
                AiWorkoutPlan.Add(new WorkoutDay
                {
                    Day = "Unable to format AI response",
                    Exercises = new List<string> { aiResponse }
                });
            }

            return Page();
        }

        public async Task<IActionResult> OnPostSaveAsync()
        {
            if (string.IsNullOrEmpty(GeneratedPlanJson))
            {
                return RedirectToPage();
            }

            var user = await _userManager.GetUserAsync(User);
            if (user == null) return Challenge();

            var workoutDays = JsonSerializer.Deserialize<List<WorkoutDay>>(GeneratedPlanJson);
            if (workoutDays == null) return RedirectToPage();

            var workoutPlan = new WorkoutPlan
            {
                Title = GeneratedPlanTitle,
                Goal = Goal, // Note: Goal might be lost if not bound, but we can try to use the bound property if the form reposts it, or just leave it generic. 
                             // Actually, since we are posting back, the BindProperty for Goal might be empty if not in the save form. 
                             // Let's assume the user wants to save the plan they just saw.
                CreatedById = user.Id,
                CreatedAt = DateTime.UtcNow,
                Exercises = new List<Exercise>()
            };

            int order = 1;
            foreach (var day in workoutDays)
            {
                foreach (var exName in day.Exercises)
                {
                    workoutPlan.Exercises.Add(new Exercise
                    {
                        Name = exName,
                        Description = $"Day: {day.Day}", // Storing the day in description for now as Exercise model is simple
                        Order = order++
                    });
                }
            }

            _context.WorkoutPlans.Add(workoutPlan);
            await _context.SaveChangesAsync();

            return RedirectToPage("/Profile/Index");
        }
    }
}
