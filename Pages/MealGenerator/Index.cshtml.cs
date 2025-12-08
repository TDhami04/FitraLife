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

        [BindProperty]
        public string GeneratedMealPlanJson { get; set; } = string.Empty;

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
                var cleanJson = aiResponse
                    .Replace("```json", "", StringComparison.OrdinalIgnoreCase)
                    .Replace("```", "")
                    .Trim();

                var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
                GeneratedMealPlan = JsonSerializer.Deserialize<MealPlan>(cleanJson, options);
                
                // Serialize for hidden input
                GeneratedMealPlanJson = JsonSerializer.Serialize(GeneratedMealPlan);
            }
            catch
            {
                // Handle error or fallback
            }

            return Page();
        }

        public async Task<IActionResult> OnPostSaveAsync()
        {
            if (string.IsNullOrEmpty(GeneratedMealPlanJson))
            {
                return RedirectToPage();
            }

            var user = await _userManager.GetUserAsync(User);
            if (user == null) return Challenge();

            var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
            var planData = JsonSerializer.Deserialize<MealPlan>(GeneratedMealPlanJson, options);
            
            if (planData == null) return RedirectToPage();

            var savedPlan = new SavedMealPlan
            {
                Title = $"{DietType} Plan ({Calories} kcal)",
                DietType = DietType,
                TargetCalories = Calories,
                TotalCalories = planData.TotalCalories,
                Protein = planData.Macros.Protein,
                Carbs = planData.Macros.Carbs,
                Fats = planData.Macros.Fats,
                CreatedById = user.Id,
                CreatedAt = DateTime.UtcNow,
                Meals = new List<SavedMealItem>()
            };

            foreach (var meal in planData.Meals)
            {
                savedPlan.Meals.Add(new SavedMealItem
                {
                    Type = meal.Type,
                    Name = meal.Name,
                    Calories = meal.Calories,
                    Description = meal.Description
                });
            }

            _context.SavedMealPlans.Add(savedPlan);
            await _context.SaveChangesAsync();

            return RedirectToPage("/Profile/Index");
        }
    }
}
