using FitraLife.Services;
using FitraLife.Data;
using FitraLife.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using System.Threading.Tasks;
using System.Text.Json;
using System.Collections.Generic;
using System;

namespace FitraLife.Pages.MealGenerator
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
        public string DietType { get; set; } = "Standard";

        [BindProperty]
        public int Calories { get; set; } = 2000;

        [BindProperty]
        public string Allergies { get; set; } = "None";

        [BindProperty]
        public string MealPlanJson { get; set; } = string.Empty;

        public MealPlan? GeneratedMealPlan { get; set; }

        public class MealPlan
        {
            public int TotalCalories { get; set; }
            public Macros Macros { get; set; } = new();
            public List<Meal> Meals { get; set; } = new();
        }

        public class Macros
        {
            public string Protein { get; set; } = "0g";
            public string Carbs { get; set; } = "0g";
            public string Fats { get; set; } = "0g";
        }

        public class Meal
        {
            public string Type { get; set; } = string.Empty;
            public string Name { get; set; } = string.Empty;
            public int Calories { get; set; }
            public string Description { get; set; } = string.Empty;
        }

        public async Task<IActionResult> OnPostGenerateAsync()
        {
            var aiResponse = await _geminiService.GenerateMealPlanAsync(DietType, Calories, Allergies);

            try
            {
                // Strip markdown if Gemini wrapped the JSON
                var cleanJson = aiResponse
                    .Replace("```json", "", StringComparison.OrdinalIgnoreCase)
                    .Replace("```", "")
                    .Trim();

                var options = new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                };

                GeneratedMealPlan = JsonSerializer.Deserialize<MealPlan>(cleanJson, options);
                
                // Serialize back to JSON to store in hidden field for saving
                MealPlanJson = JsonSerializer.Serialize(GeneratedMealPlan);
            }
            catch
            {
                // Handle error or display raw response if needed
                GeneratedMealPlan = new MealPlan
                {
                    Meals = new List<Meal>
                    {
                        new Meal { Name = "Error generating plan", Description = aiResponse }
                    }
                };
            }

            return Page();
        }

        public async Task<IActionResult> OnPostSaveAsync()
        {
            if (string.IsNullOrEmpty(MealPlanJson))
            {
                return RedirectToPage();
            }

            var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
            var plan = JsonSerializer.Deserialize<MealPlan>(MealPlanJson, options);

            if (plan != null && plan.Meals != null)
            {
                var user = await _userManager.GetUserAsync(User);
                if (user == null) return RedirectToPage("/Account/Login");

                foreach (var meal in plan.Meals)
                {
                    var log = new MealLog
                    {
                        UserId = user.Id,
                        Date = DateTime.Today,
                        MealType = meal.Type,
                        Name = meal.Name,
                        Calories = meal.Calories,
                        Description = meal.Description,
                    };
                    _context.MealLogs.Add(log);
                }

                // Update FitnessLog for today
                var fitnessLog = await _context.FitnessLogs
                    .FirstOrDefaultAsync(l => l.UserId == user.Id && l.Date == DateTime.Today);

                if (fitnessLog == null)
                {
                    fitnessLog = new FitnessLog
                    {
                        UserId = user.Id,
                        Date = DateTime.Today,
                        CaloriesEaten = 0,
                        CaloriesBurned = 0,
                        Steps = 0,
                        WorkoutMinutes = 0
                    };
                    _context.FitnessLogs.Add(fitnessLog);
                }

                // Add calories from the plan to the daily log
                fitnessLog.CaloriesEaten += plan.TotalCalories;
                
                await _context.SaveChangesAsync();
                TempData["SuccessMessage"] = "Meal plan saved to your logs!";
            }

            return RedirectToPage();
        }
    }
}
