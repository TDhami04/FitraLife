using System.Net;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using FitraLife.Services;
using Microsoft.Extensions.Configuration;

namespace FitraLife.Tests;

[TestClass]
public class GeminiServiceTests
{
    private sealed class StubHttpMessageHandler : HttpMessageHandler
    {
        private readonly Func<HttpRequestMessage, HttpResponseMessage> _handler;

        public HttpRequestMessage? LastRequest { get; private set; }

        public StubHttpMessageHandler(Func<HttpRequestMessage, HttpResponseMessage> handler)
        {
            _handler = handler;
        }

        protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            LastRequest = request;
            return Task.FromResult(_handler(request));
        }
    }

    private static IConfiguration BuildConfig(string apiKey = "test-key")
    {
        return new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["Gemini:ApiKey"] = apiKey
            })
            .Build();
    }

    private static HttpResponseMessage JsonResponse(HttpStatusCode code, string body)
    {
        return new HttpResponseMessage(code)
        {
            Content = new StringContent(body, Encoding.UTF8, "application/json")
        };
    }

    [TestMethod]
    public async Task SendChatMessageAsync_ReturnsParsedText_OnSuccess()
    {
        var json = """
        {
          "candidates": [
            {
              "content": {
                "parts": [
                  { "text": "Stay hydrated and train consistently." }
                ]
              }
            }
          ]
        }
        """;

        var handler = new StubHttpMessageHandler(_ => JsonResponse(HttpStatusCode.OK, json));
        var client = new HttpClient(handler);
        var service = new GeminiService(BuildConfig(), client);

        var result = await service.SendChatMessageAsync("Any tips?");

        Assert.AreEqual("Stay hydrated and train consistently.", result);
        Assert.IsNotNull(handler.LastRequest);
        Assert.IsTrue(handler.LastRequest!.Headers.Contains("X-goog-api-key"));
    }

    [TestMethod]
    public async Task SendChatMessageAsync_ReturnsApiError_OnNonSuccessStatus()
    {
        var handler = new StubHttpMessageHandler(_ => JsonResponse(HttpStatusCode.BadRequest, "bad request details"));
        var client = new HttpClient(handler);
        var service = new GeminiService(BuildConfig(), client);

        var result = await service.SendChatMessageAsync("hello");

        StringAssert.Contains(result, "Gemini API error");
        StringAssert.Contains(result, "BadRequest");
        StringAssert.Contains(result, "bad request details");
    }

    [TestMethod]
    public async Task SendChatMessageAsync_ReturnsFallback_OnMalformedJson()
    {
        var handler = new StubHttpMessageHandler(_ => JsonResponse(HttpStatusCode.OK, "{not-json"));
        var client = new HttpClient(handler);
        var service = new GeminiService(BuildConfig(), client);

        var result = await service.SendChatMessageAsync("hello");

        Assert.AreEqual("⚠️ I encountered an error processing your message. Please try again.", result);
    }

    [TestMethod]
    public async Task SendChatMessageAsync_UsesConversationContext_WhenProvided()
    {
        var json = """
        {
          "candidates": [
            {
              "content": {
                "parts": [
                  { "text": "ok" }
                ]
              }
            }
          ]
        }
        """;

        var handler = new StubHttpMessageHandler(_ => JsonResponse(HttpStatusCode.OK, json));
        var client = new HttpClient(handler);
        var service = new GeminiService(BuildConfig(), client);

        await service.SendChatMessageAsync("New message", "user: old\nassistant: old reply");

        var body = await handler.LastRequest!.Content!.ReadAsStringAsync();
        using var doc = JsonDocument.Parse(body);
        var prompt = doc.RootElement
            .GetProperty("contents")[0]
            .GetProperty("parts")[0]
            .GetProperty("text")
            .GetString();

        Assert.IsNotNull(prompt);
        StringAssert.Contains(prompt, "user: old");
        StringAssert.Contains(prompt, "assistant: old reply");
        StringAssert.Contains(prompt, "User: New message");
    }

    [TestMethod]
    public async Task GenerateMealPlanAsync_ReturnsNoMealPlanMessage_WhenTextIsNull()
    {
        var json = """
        {
          "candidates": [
            {
              "content": {
                "parts": [
                  { "text": null }
                ]
              }
            }
          ]
        }
        """;

        var handler = new StubHttpMessageHandler(_ => JsonResponse(HttpStatusCode.OK, json));
        var client = new HttpClient(handler);
        var service = new GeminiService(BuildConfig(), client);

        var result = await service.GenerateMealPlanAsync("Standard", 2000, "None");

        Assert.AreEqual("No meal plan generated.", result);
    }

    [TestMethod]
    public async Task GenerateWorkoutPlanAsync_ReturnsParseError_OnInvalidPayloadShape()
    {
        var json = """
        {
          "unexpected": []
        }
        """;

        var handler = new StubHttpMessageHandler(_ => JsonResponse(HttpStatusCode.OK, json));
        var client = new HttpClient(handler);
        var service = new GeminiService(BuildConfig(), client);

        var result = await service.GenerateWorkoutPlanAsync("Lose", "Beginner", "3");

        StringAssert.Contains(result, "Error parsing Gemini response");
        StringAssert.Contains(result, "Raw response");
    }
}
