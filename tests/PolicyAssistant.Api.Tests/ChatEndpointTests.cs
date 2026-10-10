using System.Net;
using System.Net.Http.Json;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.DependencyInjection;
using NSubstitute;
using PolicyAssistant.Api.Chat;
using PolicyAssistant.Shared;

namespace PolicyAssistant.Api.Tests;

public class ChatEndpointTests : IClassFixture<ChatEndpointTests.ApiFactory>
{
    private readonly ApiFactory _factory;

    public ChatEndpointTests(ApiFactory factory)
    {
        _factory = factory;
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public async Task EmptyQuestion_Returns400(string question)
    {
        var response = await PostChat(new ChatRequest(question));

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task TooLongQuestion_Returns400()
    {
        var response = await PostChat(new ChatRequest(new string('a', 1001)));

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task ValidQuestion_Returns200_WithChatServiceResponse()
    {
        var expected = new ChatResponse("Answer [1]", [new Citation(1, "Leave policy", "snippet", "chunk-1")], IsGrounded: true);
        _factory.ChatService.AskAsync(Arg.Any<ChatRequest>(), Arg.Any<CancellationToken>()).Returns(expected);

        var response = await PostChat(new ChatRequest("How much leave do I get?"));

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var body = await response.Content.ReadFromJsonAsync<ChatResponse>();
        Assert.Equal(expected.Answer, body!.Answer);
        Assert.Equal(expected.Citations, body.Citations);
    }

    private Task<HttpResponseMessage> PostChat(ChatRequest request) =>
        _factory.CreateClient().PostAsJsonAsync("/chat", request);

    public sealed class ApiFactory : WebApplicationFactory<Program>
    {
        public IChatService ChatService { get; } = Substitute.For<IChatService>();

        protected override void ConfigureWebHost(IWebHostBuilder builder)
        {
            // Placeholder values so options validation passes; no Azure calls are made.
            builder.UseSetting("AzureSearch:Endpoint", "https://test.search.windows.net");
            builder.UseSetting("AzureSearch:IndexName", "test-index");
            builder.UseSetting("AzureSearch:SemanticConfigurationName", "default");
            builder.UseSetting("AzureOpenAI:Endpoint", "https://test.openai.azure.com/openai/v1");
            builder.UseSetting("AzureOpenAI:ChatDeployment", "test-model");

            builder.ConfigureTestServices(services => services.AddSingleton(ChatService));
        }
    }
}
