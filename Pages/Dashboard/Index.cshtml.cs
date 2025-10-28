using System.Threading.Tasks;
using FitraLife.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc.RazorPages;
using FitraLife.Data;
using SQLitePCL;

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

        // Insights properties
        public double AverageSteps { get; set; }
        public double AverageCaloriesBurned { get; set; }
        public double AverageCaloriesEaten { get; set; }
        public double TotalWorkoutMinutes { get; set; }
        public string StepTrend { get; set; } = "No data";

        // Weekly Goal Properties
        public double WeeklySteps { get; set; }
        public double WeeklyCaloriesBurned { get; set; }
        public double WeeklyWorkoutMinutes { get; set; }

        // Set default goals (you can later make these customizable per user)
        public int StepGoal { get; set; } = 70000; // 10k steps * 7 days
        public int CaloriesBurnedGoal { get; set; } = 3500; // e.g. 500 per day
        public int WorkoutGoal { get; set; } = 300; // e.g. 300 minutes per week (about 40 min/day)

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
                .Take(10)
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
            WeeklyWorkoutMinutes = weeklyLogs.Sum(f => f.WorkoutMinutes);
        }
    }
}
