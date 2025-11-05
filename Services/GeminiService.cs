using System;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;

namespace FitraLife.Services
{
    public class GeminiService : IGeminiService
    {
        private readonly HttpClient _httpClient;
        private readonly string _apiKey;

        public GeminiService(IConfiguration config)
        {
            _apiKey = config["Gemini:ApiKey"] ?? throw new ArgumentNullException("Gemini:ApiKey not found in configuration.");
            _httpClient = new HttpClient();
        }

        public async Task<string> GenerateWorkoutPlanAsync(string userGoal, string experienceLevel, string availableDays)
        {
            var prompt = $@"
                        Generate a weekly structured workout plan for a user with these details:
                        - Goal: {userGoal}
                        - Experience level: {experienceLevel}
                        - Days available per week: {availableDays}

                        Return ONLY a valid JSON object. 
                        Do NOT include markdown, backticks, explanations, or any extra text.
                        Output only pure JSON in this format:

                        {{
                        ""planTitle"": ""string"",
                        ""days"": [
                            {{
                            ""day"": ""string"",
                            ""exercises"": [""string"", ""string""]
                            }}
                        ]
                        }}
                        ";


            // Gemini API request format
            var requestBody = new
            {
                contents = new[]
                {
                    new
                    {
                        parts = new[]
                        {
                            new { text = prompt }
                        }
                    }
                }
            };

            var requestJson = JsonSerializer.Serialize(requestBody);

            var request = new HttpRequestMessage(HttpMethod.Post,
                "https://generativelanguage.googleapis.com/v1beta/models/gemini-2.0-flash:generateContent")
            {
                Content = new StringContent(requestJson, Encoding.UTF8, "application/json")
            };

            // Use header-based key (new Gemini requirement)
            request.Headers.Add("X-goog-api-key", _apiKey);

            // Send request
            var response = await _httpClient.SendAsync(request);
            var responseString = await response.Content.ReadAsStringAsync();

            if (!response.IsSuccessStatusCode)
            {
                return $"⚠️ Gemini API error ({response.StatusCode}):\n{responseString}";
            }

            try
            {
                using var json = JsonDocument.Parse(responseString);
                var text = json
                    .RootElement
                    .GetProperty("candidates")[0]
                    .GetProperty("content")
                    .GetProperty("parts")[0]
                    .GetProperty("text")
                    .GetString();

                return text ?? "No workout plan generated.";
            }
            catch (Exception ex)
            {
                return $"⚠️ Error parsing Gemini response: {ex.Message}\n\nRaw response:\n{responseString}";
            }
        }
    }
}
