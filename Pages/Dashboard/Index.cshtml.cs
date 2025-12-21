using System;
using System.Linq;
using System.Collections.Generic;
using System.Threading.Tasks;
using FitraLife.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc.RazorPages;
using FitraLife.Data;
using FitraLife.Services;

namespace FitraLife.Pages.Dashboard
{
    [Authorize]
    public class IndexModel : PageModel
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly ApplicationDbContext _context;
        private readonly IAnalyticsService _analyticsService;

        public ApplicationUser? CurrentUser { get; set; }
        public FitnessLog? LatestLog { get; set; }
        public List<FitnessLog> FitnessLogs { get; set; } = new();
        public WeightPredictionResult? Prediction { get; set; }

        public double AverageSteps { get; set; }
        public double AverageCaloriesBurned { get; set; }
        public double AverageCaloriesEaten { get; set; }
        public double TotalWorkoutMinutes { get; set; }
        public string StepTrend { get; set; } = "No data";

        public double WeeklySteps { get; set; }
        public double WeeklyCaloriesBurned { get; set; }
        public double WeeklyCaloriesEaten { get; set; }
        public double WeeklyWorkoutMinutes { get; set; }

        public int StepGoal { get; set; }
        public int WorkoutMinutesGoal { get; set; }
        public double WeeklyEatGoal { get; set; }
        public double EstimatedBMR { get; set; }
        public double EstimatedTDEE { get; set; }

        public string WeeklyFeedback { get; set; } = string.Empty;

        public IndexModel(UserManager<ApplicationUser> userManager, ApplicationDbContext context, IAnalyticsService analyticsService)
        {
            _userManager = userManager;
            _context = context;
            _analyticsService = analyticsService;
        }

        public async Task OnGetAsync()
        {
            CurrentUser = await _userManager.GetUserAsync(User);
            if (CurrentUser == null) return;

            StepGoal = CurrentUser.StepGoal;
            WorkoutMinutesGoal = CurrentUser.WorkoutMinutesGoal;

            // Get all logs for the user
            FitnessLogs = _context.FitnessLogs
                .Where(f => f.UserId == CurrentUser.Id)
                .OrderByDescending(f => f.Date)
                .Take(30)
                .ToList();

            // Calculate BMR/TDEE first so we can pass BMR to the prediction engine
            CalculateEnergyExpenditure(CurrentUser);

            // Generate Prediction (Now passing BMR)
            Prediction = _analyticsService.PredictGoalDate(FitnessLogs, CurrentUser.Weight, CurrentUser.TargetWeight, EstimatedBMR);

            // Get logs specifically for today (using local date)
            var today = DateTime.Now.Date;
            var todaysLogs = _context.FitnessLogs
                .Where(f => f.UserId == CurrentUser.Id && f.Date.Date == today)
                .ToList();

            if (todaysLogs.Any())
            {
                // Combine all today's entries into one daily summary
                LatestLog = new FitnessLog
                {
                    Date = today,
                    Steps = todaysLogs.Sum(l => l.Steps),
                    CaloriesBurned = todaysLogs.Sum(l => l.CaloriesBurned),
                    CaloriesEaten = todaysLogs.Sum(l => l.CaloriesEaten),
                    WorkoutMinutes = todaysLogs.Sum(l => l.WorkoutMinutes)
                };
            }
            else
            {
                LatestLog = null; // no logs today, display "No logs for today"
            }

            // Averages + Trends
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
                    StepTrend = change > 0 ? $"⬆ {change} steps vs previous" :
                                change < 0 ? $"⬇ {Math.Abs(change)} steps vs previous" :
                                "No change from previous day";
                }
            }

            // Weekly calculations
            var weekStart = DateTime.Today.AddDays(-7);
            var weeklyLogs = FitnessLogs.Where(f => f.Date >= weekStart).ToList();

            WeeklySteps = weeklyLogs.Sum(f => f.Steps);
            WeeklyCaloriesBurned = weeklyLogs.Sum(f => f.CaloriesBurned);
            WeeklyCaloriesEaten = weeklyLogs.Sum(f => f.CaloriesEaten);
            WeeklyWorkoutMinutes = weeklyLogs.Sum(f => f.WorkoutMinutes);

            WeeklyEatGoal = CalculateWeeklyEatGoal(CurrentUser);
            WeeklyFeedback = GenerateFeedback(CurrentUser);
        }

        private void CalculateEnergyExpenditure(ApplicationUser user)
        {
            if (user == null || user.Weight <= 0 || user.Height <= 0 || user.Age <= 0) return;

            // BMR (Mifflin-St Jeor Equation)
            EstimatedBMR = user.Gender == "Male"
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

            EstimatedTDEE = EstimatedBMR * activity;
        }

        private double CalculateWeeklyEatGoal(ApplicationUser user)
        {
            if (EstimatedTDEE <= 0) return 0;

            double dailyGoal = EstimatedTDEE;

            switch (user.GoalType)
            {
                case "Lose": dailyGoal -= 500; break;
                case "Gain": dailyGoal += 500; break;
            }

            return dailyGoal * 7; // weekly goal
        }

        private string GenerateFeedback(ApplicationUser user)
        {
            if (user == null) return string.Empty;

            var stepPercent = StepGoal > 0 ? Math.Min(100, WeeklySteps / StepGoal * 100) : 0;
            var workoutPercent = WorkoutMinutesGoal > 0 ? Math.Min(100, WeeklyWorkoutMinutes / WorkoutMinutesGoal * 100) : 0;

            if (user.GoalType == "Gain")
            {
                if (stepPercent < 40 || workoutPercent < 40)
                    return "Try adding a structured workout or more daily steps to support muscle gain.";
                if (stepPercent < 80 || workoutPercent < 80)
                    return "Good progress — keep consistent and increase protein intake.";
                return "Nice — you’re consistent this week, keep it up!";
            }
            else if (user.GoalType == "Lose")
            {
                if (stepPercent < 40 || workoutPercent < 40)
                    return "Add a couple short cardio sessions to boost calorie burn this week.";
                if (stepPercent < 80 || workoutPercent < 80)
                    return "On track — keep the momentum. Small increases to daily steps can help.";
                return "Excellent — you’re making solid progress toward your burn goals.";
            }

            if (stepPercent < 60 || workoutPercent < 60)
                return "Staying active is key — small daily steps add up. Try a short routine 3× weekly.";
            return "Nice stability — you’re maintaining activity levels well.";
        }
    }
}
