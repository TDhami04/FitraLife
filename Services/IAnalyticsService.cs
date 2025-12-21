using System.Collections.Generic;
using FitraLife.Models;

namespace FitraLife.Services
{
    public interface IAnalyticsService
    {
        WeightPredictionResult PredictGoalDate(List<FitnessLog> recentLogs, double currentWeight, double targetWeight, double bmr);
    }

    public class WeightPredictionResult
    {
        public DateTime? PredictedDate { get; set; }
        public double DailyCalorieDeficit { get; set; }
        public double ProjectedWeeklyLossKg { get; set; }
        public string Message { get; set; } = string.Empty;
        public bool IsOnTrack { get; set; }
    }
}
