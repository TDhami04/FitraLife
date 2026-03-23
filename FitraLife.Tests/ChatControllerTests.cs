using System.Security.Claims;
using FitraLife.Api.Controllers;
using FitraLife.Data;
using FitraLife.Models;
using FitraLife.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;
using Moq;

namespace FitraLife.Tests;

[TestClass]
public class ChatControllerTests
{
    private static ApplicationDbContext CreateContext(string dbName)
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(dbName)
            .Options;

        return new ApplicationDbContext(options);
    }

    private static UserManager<ApplicationUser> CreateUserManager()
    {
        var store = new Mock<IUserStore<ApplicationUser>>();
        var services = new ServiceCollection().BuildServiceProvider();
        return new UserManager<ApplicationUser>(
            store.Object,
            Options.Create(new IdentityOptions()),
            new PasswordHasher<ApplicationUser>(),
            Array.Empty<IUserValidator<ApplicationUser>>(),
            Array.Empty<IPasswordValidator<ApplicationUser>>(),
            new UpperInvariantLookupNormalizer(),
            new IdentityErrorDescriber(),
            services,
            NullLogger<UserManager<ApplicationUser>>.Instance);
    }

    private static ChatController CreateController(
        IGeminiService geminiService,
        ApplicationDbContext context,
        UserManager<ApplicationUser> userManager,
        string? userId = "user-1")
    {
        var controller = new ChatController(geminiService, context, userManager);

        ClaimsPrincipal principal;
        if (string.IsNullOrEmpty(userId))
        {
            principal = new ClaimsPrincipal(new ClaimsIdentity());
        }
        else
        {
            var identity = new ClaimsIdentity(new[]
            {
                new Claim(ClaimTypes.NameIdentifier, userId)
            }, "TestAuth");
            principal = new ClaimsPrincipal(identity);
        }

        controller.ControllerContext = new ControllerContext
        {
            HttpContext = new DefaultHttpContext
            {
                User = principal
            }
        };

        return controller;
    }

    [TestMethod]
    public async Task SendMessage_ReturnsBadRequest_WhenMessageIsEmpty()
    {
        using var context = CreateContext(nameof(SendMessage_ReturnsBadRequest_WhenMessageIsEmpty));
        var gemini = new Mock<IGeminiService>();
        var userManager = CreateUserManager();
        var controller = CreateController(gemini.Object, context, userManager);

        var result = await controller.SendMessage(new ChatRequest { Message = "   " });

        Assert.IsInstanceOfType<BadRequestObjectResult>(result);
    }

    [TestMethod]
    public async Task SendMessage_ReturnsUnauthorized_WhenUserIsMissing()
    {
        using var context = CreateContext(nameof(SendMessage_ReturnsUnauthorized_WhenUserIsMissing));
        var gemini = new Mock<IGeminiService>();
        var userManager = CreateUserManager();
        var controller = CreateController(gemini.Object, context, userManager, null);

        var result = await controller.SendMessage(new ChatRequest { Message = "Hello" });

        Assert.IsInstanceOfType<UnauthorizedResult>(result);
    }

    [TestMethod]
    public async Task SendMessage_ReturnsNotFound_WhenSessionDoesNotExist()
    {
        using var context = CreateContext(nameof(SendMessage_ReturnsNotFound_WhenSessionDoesNotExist));
        var gemini = new Mock<IGeminiService>();
        var userManager = CreateUserManager();
        var controller = CreateController(gemini.Object, context, userManager);

        var result = await controller.SendMessage(new ChatRequest
        {
            Message = "Hello",
            SessionId = 999
        });

        Assert.IsInstanceOfType<NotFoundObjectResult>(result);
    }

    [TestMethod]
    public async Task SendMessage_CreatesSessionAndPersistsMessages_WhenNewSession()
    {
        using var context = CreateContext(nameof(SendMessage_CreatesSessionAndPersistsMessages_WhenNewSession));
        var gemini = new Mock<IGeminiService>();
        gemini
            .Setup(g => g.SendChatMessageAsync(It.IsAny<string>(), It.IsAny<string>()))
            .ReturnsAsync("AI reply");

        var userManager = CreateUserManager();
        var controller = CreateController(gemini.Object, context, userManager, "user-42");

        var result = await controller.SendMessage(new ChatRequest { Message = "Create me a plan" });

        Assert.IsInstanceOfType<OkObjectResult>(result);
        var session = await context.ChatSessions.SingleAsync();
        Assert.AreEqual("user-42", session.UserId);
        Assert.AreEqual("Create me a plan", session.Title);

        var messages = await context.ChatMessages
            .Where(m => m.ChatSessionId == session.Id)
            .OrderBy(m => m.Timestamp)
            .ToListAsync();

        Assert.AreEqual(2, messages.Count);
        Assert.AreEqual("user", messages[0].Role);
        Assert.AreEqual("assistant", messages[1].Role);
        Assert.AreEqual("AI reply", messages[1].Content);
    }

    [TestMethod]
    public async Task SendMessage_ReturnsServerError_WhenGeminiThrows()
    {
        using var context = CreateContext(nameof(SendMessage_ReturnsServerError_WhenGeminiThrows));
        var gemini = new Mock<IGeminiService>();
        gemini
            .Setup(g => g.SendChatMessageAsync(It.IsAny<string>(), It.IsAny<string>()))
            .ThrowsAsync(new InvalidOperationException("boom"));

        var userManager = CreateUserManager();
        var controller = CreateController(gemini.Object, context, userManager);

        var result = await controller.SendMessage(new ChatRequest { Message = "Hello" });

        Assert.IsInstanceOfType<ObjectResult>(result);
        var objectResult = (ObjectResult)result;
        Assert.AreEqual(500, objectResult.StatusCode);
    }

    [TestMethod]
    public async Task DeleteSession_RemovesOwnedSession()
    {
        using var context = CreateContext(nameof(DeleteSession_RemovesOwnedSession));

        context.ChatSessions.Add(new ChatSession
        {
            Id = 1,
            UserId = "user-1",
            Title = "Session",
            CreatedAt = DateTime.UtcNow
        });
        await context.SaveChangesAsync();

        var gemini = new Mock<IGeminiService>();
        var userManager = CreateUserManager();
        var controller = CreateController(gemini.Object, context, userManager, "user-1");

        var result = await controller.DeleteSession(1);

        Assert.IsInstanceOfType<OkObjectResult>(result);
        Assert.AreEqual(0, await context.ChatSessions.CountAsync());
    }
}
