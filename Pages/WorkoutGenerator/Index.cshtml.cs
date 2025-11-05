using FitraLife.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Authorization;
using System.Threading.Tasks;
using System.Text.RegularExpressions;
using System.Text.Json;

namespace FitraLife.Pages.WorkoutGenerator
{
    [Authorize]
    public class IndexModel : PageModel
    {
        private readonly IGeminiService _geminiService;

        public IndexModel(IGeminiService geminiService)
        {
            _geminiService = geminiService;
        }

        [BindProperty]
        public string Goal { get; set; } = string.Empty;

        [BindProperty]
        public string ExperienceLevel { get; set; } = "Beginner";

        [BindProperty]
        public string AvailableDays { get; set; } = "3";

        public string PlanTitle { get; set; } = string.Empty;
        public List<WorkoutDay> AiWorkoutPlan { get; set; } = new();

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

    }
}
