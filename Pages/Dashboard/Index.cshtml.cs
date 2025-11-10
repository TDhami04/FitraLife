using System;
using System.Linq;
using System.Threading.Tasks;
using FitraLife.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc.RazorPages;
using FitraLife.Data;

namespace FitraLife.Pages.Dashboard
{
    [Authorize]
    public class IndexModel : PageModel
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly ApplicationDbContext _context;

        public ApplicationUser? CurrentUser { get; set; }
        public FitnessLog? LatestLog { get; set; }
        public List<FitnessLog> FitnessLogs { get; set; } = new();

        public double AverageSteps { get; set; }
        public double AverageCaloriesBurned { get; set; }
        public double AverageCaloriesEaten { get; set; }
        public double TotalWorkoutMinutes { get; set; }
        public string StepTrend { get; set; } = "No data";

        public double WeeklySteps { get; set; }
        public double WeeklyCaloriesBurned { get; set; }
        public double WeeklyWorkoutMinutes { get; set; }

    public int StepGoal { get; set; } = 70000;
    public int WorkoutGoal { get; set; } = 300;

    public double WeeklyEatGoal { get; set; }
    public double WeeklyCaloriesEaten { get; set; }

        public IndexModel(UserManager<ApplicationUser> userManager, ApplicationDbContext context)
        {
            _userManager = userManager;
            _context = context;
        }

        public async Task OnGetAsync()
        {
            CurrentUser = await _userManager.GetUserAsync(User);
            if (CurrentUser == null) return;

            LatestLog = _context.FitnessLogs
                .Where(f => f.UserId == CurrentUser.Id)
                .OrderByDescending(f => f.Date)
                .FirstOrDefault();

            FitnessLogs = _context.FitnessLogs
                .Where(f => f.UserId == CurrentUser.Id)
                .OrderByDescending(f => f.Date)
                .Take(30)
                .ToList();

            if (FitnessLogs.Any())
            {
                AverageSteps = FitnessLogs.Average(f => f.Steps);
                AverageCaloriesBurned = FitnessLogs.Average(f => f.CaloriesBurned);
                AverageCaloriesEaten = FitnessLogs.Average(f => f.CaloriesEaten);
                TotalWorkoutMinutes = FitnessLogs.Sum(f => f.WorkoutMinutes);

                if (FitnessLogs.Count >= 2)
                {
                    var last = FitnessLogs.First().Steps;
                    var previous = FitnessLogs.Skip(1).First().Steps;
                    var change = last - previous;
                    StepTrend = change > 0 ? $"⬆ {change} steps vs. previous" :
                                change < 0 ? $"⬇ {Math.Abs(change)} steps vs. previous" :
                                "No change from previous day";
                }
            }

            var weekStart = DateTime.Now.AddDays(-7);
            var weeklyLogs = FitnessLogs.Where(f => f.Date >= weekStart).ToList();

            WeeklySteps = weeklyLogs.Sum(f => f.Steps);
            WeeklyCaloriesBurned = weeklyLogs.Sum(f => f.CaloriesBurned);
            WeeklyCaloriesEaten = weeklyLogs.Sum(f => f.CaloriesEaten);
            WeeklyWorkoutMinutes = weeklyLogs.Sum(f => f.WorkoutMinutes);

            WeeklyEatGoal = CalculateWeeklyCalorieGoal(CurrentUser, "eat");
        }

        private double CalculateWeeklyCalorieGoal(ApplicationUser user, string type)
        {
            if (user.Weight <= 0 || user.Height <= 0 || user.Age <= 0)
                return 0;

            // BMR (Mifflin-St Jeor Equation)
            double bmr = user.Gender == "Male"
                ? 10 * user.Weight + 6.25 * user.Height - 5 * user.Age + 5
                : 10 * user.Weight + 6.25 * user.Height - 5 * user.Age - 161;

            // Activity factor
            double activity = user.ActivityLevel switch
            {
                "Sedentary" => 1.2,
                "Light" => 1.375,
                "Moderate" => 1.55,
                "Active" => 1.725,
                _ => 1.55
            };

            double maintenance = bmr * activity;
            double dailyGoal = maintenance;

            switch (user.GoalType)
            {
                case "Lose":
                    dailyGoal -= 500;
                    break;
                case "Gain":
                    dailyGoal += 500;
                    break;
            }

            if (type == "eat") return dailyGoal * 7;
            // 'burn' calculation removed from the page model — the view computes burn goal inline when needed.
            return 0;
        }
    }
}
