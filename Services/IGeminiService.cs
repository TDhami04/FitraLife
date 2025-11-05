using FitraLife.Models;

namespace FitraLife.Services
{
    public interface IGeminiService
    {
        Task<string> GenerateWorkoutPlanAsync(string userGoal, string experienceLevel, string availableDays);
    }
}