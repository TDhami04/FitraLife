using FitraLife.Models;

namespace FitraLife.Services
{
    public interface IGeminiService
    {
        Task<string> GenerateWorkoutPlanAsync(string userGoal, string experienceLevel, string availableDays);
        Task<string> GenerateMealPlanAsync(string dietType, int calories, string allergies);
        Task<string> SendChatMessageAsync(string userMessage, string conversationContext = "");
    }
}