using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using FitraLife.Services;
using System.Text.Json;

namespace FitraLife.Api.Controllers
{
    [Authorize]
    [Route("api/[controller]")]
    [ApiController]
    public class ChatController : ControllerBase
    {
        private readonly IGeminiService _geminiService;

        public ChatController(IGeminiService geminiService)
        {
            _geminiService = geminiService;
        }

        [HttpPost("send")]
        public async Task<IActionResult> SendMessage([FromBody] ChatRequest request)
        {
            if (string.IsNullOrWhiteSpace(request.Message))
            {
                return BadRequest(new { error = "Message cannot be empty" });
            }

            try
            {
                // Get chat history from session
                var sessionHistory = HttpContext.Session.GetString("ChatHistory");
                var chatHistory = string.IsNullOrEmpty(sessionHistory)
                    ? new List<ChatMessage>()
                    : JsonSerializer.Deserialize<List<ChatMessage>>(sessionHistory) ?? new();

                // Build conversation context
                var conversationContext = string.Join("\n",
                    chatHistory.TakeLast(5).Select(m => $"{m.Role}: {m.Content}"));

                // Get AI response
                var aiResponse = await _geminiService.SendChatMessageAsync(request.Message, conversationContext);

                // Update history
                chatHistory.Add(new ChatMessage { Role = "user", Content = request.Message, Timestamp = DateTime.Now });
                chatHistory.Add(new ChatMessage { Role = "assistant", Content = aiResponse, Timestamp = DateTime.Now });

                // Save to session
                HttpContext.Session.SetString("ChatHistory", JsonSerializer.Serialize(chatHistory));

                return Ok(new {
                    response = aiResponse,
                    timestamp = DateTime.Now
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { error = "Failed to process message", details = ex.Message });
            }
        }

        [HttpPost("clear")]
        public IActionResult ClearHistory()
        {
            HttpContext.Session.Remove("ChatHistory");
            return Ok(new { message = "Chat history cleared" });
        }

        [HttpGet("history")]
        public IActionResult GetHistory()
        {
            var sessionHistory = HttpContext.Session.GetString("ChatHistory");
            var chatHistory = string.IsNullOrEmpty(sessionHistory)
                ? new List<ChatMessage>()
                : JsonSerializer.Deserialize<List<ChatMessage>>(sessionHistory) ?? new();

            return Ok(chatHistory);
        }
    }
}