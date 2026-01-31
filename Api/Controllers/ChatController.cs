using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using FitraLife.Services;
using FitraLife.Data;
using FitraLife.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace FitraLife.Api.Controllers
{
    [Authorize]
    [Route("api/[controller]")]
    [ApiController]
    public class ChatController : ControllerBase
    {
        private readonly IGeminiService _geminiService;
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;

        public ChatController(IGeminiService geminiService, ApplicationDbContext context, UserManager<ApplicationUser> userManager)
        {
            _geminiService = geminiService;
            _context = context;
            _userManager = userManager;
        }

        // GET: api/Chat/sessions
        [HttpGet("sessions")]
        public async Task<IActionResult> GetSessions()
        {
            var userId = _userManager.GetUserId(User);
            if (userId == null) return Unauthorized();

            var sessions = await _context.ChatSessions
                .Where(s => s.UserId == userId)
                .OrderByDescending(s => s.CreatedAt)
                .Select(s => new { s.Id, s.Title, s.CreatedAt })
                .ToListAsync();

            return Ok(sessions);
        }

        // GET: api/Chat/session/{id}
        [HttpGet("session/{id}")]
        public async Task<IActionResult> GetSession(int id)
        {
            var userId = _userManager.GetUserId(User);
            if (userId == null) return Unauthorized();

            var session = await _context.ChatSessions
                .Include(s => s.Messages)
                .FirstOrDefaultAsync(s => s.Id == id && s.UserId == userId);

            if (session == null) return NotFound();

            return Ok(session.Messages.OrderBy(m => m.Timestamp).Select(m => new { m.Role, m.Content, m.Timestamp }));
        }

        // POST: api/Chat/send
        [HttpPost("send")]
        public async Task<IActionResult> SendMessage([FromBody] ChatRequest request)
        {
            if (string.IsNullOrWhiteSpace(request.Message))
                return BadRequest(new { error = "Message cannot be empty" });

            var userId = _userManager.GetUserId(User);
            if (userId == null) return Unauthorized();

            ChatSession? session = null;

            if (request.SessionId.HasValue)
            {
                session = await _context.ChatSessions
                    .Include(s => s.Messages)
                    .FirstOrDefaultAsync(s => s.Id == request.SessionId && s.UserId == userId);
                
                if (session == null) return NotFound("Session not found");
            }
            else
            {
                // Create new session
                session = new ChatSession
                {
                    UserId = userId,
                    Title = request.Message.Length > 30 ? request.Message.Substring(0, 30) + "..." : request.Message,
                    CreatedAt = DateTime.Now
                };
                _context.ChatSessions.Add(session);
                await _context.SaveChangesAsync(); // Save to get Id
            }

            try
            {
                // Build context from last 10 messages
                var history = session.Messages.OrderBy(m => m.Timestamp).TakeLast(10).ToList();
                var conversationContext = string.Join("\n", history.Select(m => $"{m.Role}: {m.Content}"));

                // Get AI Response
                var aiResponse = await _geminiService.SendChatMessageAsync(request.Message, conversationContext);

                // Save User Message
                var userMsg = new ChatMessage
                {
                    ChatSessionId = session.Id,
                    Role = "user",
                    Content = request.Message,
                    Timestamp = DateTime.Now
                };
                _context.ChatMessages.Add(userMsg);

                // Save AI Message
                var aiMsg = new ChatMessage
                {
                    ChatSessionId = session.Id,
                    Role = "assistant",
                    Content = aiResponse,
                    Timestamp = DateTime.Now.AddSeconds(1) // Ensure order
                };
                _context.ChatMessages.Add(aiMsg);

                await _context.SaveChangesAsync();

                return Ok(new { 
                    response = aiResponse, 
                    sessionId = session.Id,
                    title = session.Title
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { error = "Failed to process message", details = ex.Message });
            }
        }

        // DELETE: api/Chat/session/{id}
        [HttpDelete("session/{id}")]
        public async Task<IActionResult> DeleteSession(int id)
        {
            var userId = _userManager.GetUserId(User);
            if (userId == null) return Unauthorized();

            var session = await _context.ChatSessions
                .FirstOrDefaultAsync(s => s.Id == id && s.UserId == userId);

            if (session == null) return NotFound();

            _context.ChatSessions.Remove(session);
            await _context.SaveChangesAsync();

            return Ok(new { message = "Session deleted" });
        }
    }
}