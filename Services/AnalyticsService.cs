using System;
using System.Collections.Generic;
using System.Linq;
using FitraLife.Models;

namespace FitraLife.Services
{
    public class AnalyticsService : IAnalyticsService
    {
        public WeightPredictionResult PredictGoalDate(List<FitnessLog> recentLogs, double currentWeight, double targetWeight, double bmr)
        {
            // 1. Data Validation
            if (recentLogs == null || !recentLogs.Any())
            {
                return new WeightPredictionResult { Message = "Not enough data to make a prediction. Log your activity for a few days!" };
            }

            if (targetWeight <= 0)
            {
                 return new WeightPredictionResult { Message = "Please set a target weight in your profile to see predictions." };
            }

            // 2. Calculate Average "Net Calories" (Eaten - Burned)
            double totalNetCalories = 0;
            foreach (var log in recentLogs)
            {
                // Assuming the user logs ACTIVE calories only.
                // So Total Burned = BMR + Active Calories (log.CaloriesBurned)
                double totalDailyBurn = bmr + log.CaloriesBurned;
                
                totalNetCalories += (log.CaloriesEaten - totalDailyBurn);
            }
            
            double averageDailyNet = totalNetCalories / recentLogs.Count;

            // 3. The "Algorithm"
            // ~7700 calories is roughly 1kg of body weight change (fat or muscle mix)
            const double CaloriesPerKg = 7700.0;
            
            // Calculate expected daily weight change in kg
            double dailyWeightChangeKg = averageDailyNet / CaloriesPerKg;

            bool isAimingToLose = targetWeight < currentWeight;
            bool isAimingToGain = targetWeight > currentWeight;

            // 5. Check if they are on the right track
            if (isAimingToLose && dailyWeightChangeKg >= 0)
            {
                return new WeightPredictionResult 
                { 
                    IsOnTrack = false,
                    DailyCalorieDeficit = -averageDailyNet,
                    Message = "You are aiming to lose weight, but you are currently in a calorie surplus. You are projected to GAIN weight." 
                };
            }

            if (isAimingToGain && dailyWeightChangeKg <= 0)
            {
                return new WeightPredictionResult 
                { 
                    IsOnTrack = false,
                    DailyCalorieDeficit = -averageDailyNet,
                    Message = "You are aiming to gain weight, but you are currently in a calorie deficit. You are projected to LOSE weight." 
                };
            }

            // 6. Calculate Days to Goal
            double weightDifference = Math.Abs(targetWeight - currentWeight);
            double dailyChangeMagnitude = Math.Abs(dailyWeightChangeKg);

            // Avoid division by zero if they are perfectly maintaining
            if (dailyChangeMagnitude < 0.001) 
            {
                return new WeightPredictionResult 
                { 
                    IsOnTrack = true,
                    Message = "You are currently maintaining your weight perfectly." 
                };
            }

            int daysToGoal = (int)Math.Ceiling(weightDifference / dailyChangeMagnitude);

            // Cap the prediction to avoid "In 50 years" messages
            if (daysToGoal > 365 * 2)
            {
                 return new WeightPredictionResult
                {
                    IsOnTrack = true,
                    DailyCalorieDeficit = -averageDailyNet,
                    ProjectedWeeklyLossKg = dailyWeightChangeKg * 7,
                    Message = "At this rate, it will take more than 2 years. Try adjusting your calories for faster results."
                };
            }

            // 7. The Result
            return new WeightPredictionResult
            {
                IsOnTrack = true,
                PredictedDate = DateTime.Today.AddDays(daysToGoal),
                DailyCalorieDeficit = -averageDailyNet, // Display as deficit (positive for loss)
                ProjectedWeeklyLossKg = dailyWeightChangeKg * 7, 
                Message = $"On track to reach {targetWeight}kg by {DateTime.Today.AddDays(daysToGoal):MMMM dd, yyyy}"
            };
        }
    }
}
