using System.ClientModel.Primitives;
using Azure.Core;
using Azure.Identity;
using Azure.Search.Documents;
using Microsoft.Extensions.Options;
using OpenAI;
using OpenAI.Chat;
using PolicyAssistant.Api.Chat;
using PolicyAssistant.Api.Generation;
using PolicyAssistant.Api.Options;
using PolicyAssistant.Api.Retrieval;
using PolicyAssistant.Shared;

const int MaxQuestionLength = 1000;
const string WebCorsPolicy = "Web";

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddOptions<AzureSearchOptions>()
    .BindConfiguration(AzureSearchOptions.SectionName)
    .ValidateDataAnnotations()
    .ValidateOnStart();
builder.Services.AddOptions<AzureOpenAIOptions>()
    .BindConfiguration(AzureOpenAIOptions.SectionName)
    .ValidateDataAnnotations()
    .ValidateOnStart();

builder.Services.AddSingleton<TokenCredential>(_ => new DefaultAzureCredential());

builder.Services.AddSingleton(sp =>
{
    var options = sp.GetRequiredService<IOptions<AzureSearchOptions>>().Value;
    return new SearchClient(new Uri(options.Endpoint), options.IndexName, sp.GetRequiredService<TokenCredential>());
});

builder.Services.AddSingleton(sp =>
{
    var options = sp.GetRequiredService<IOptions<AzureOpenAIOptions>>().Value;
    var tokenPolicy = new BearerTokenPolicy(sp.GetRequiredService<TokenCredential>(), "https://ai.azure.com/.default");
    // Entra ID auth (no API key) on the Foundry v1 endpoint; the SDK still marks this constructor experimental.
#pragma warning disable OPENAI001
    var client = new OpenAIClient(tokenPolicy, new OpenAIClientOptions { Endpoint = new Uri(options.Endpoint) });
#pragma warning restore OPENAI001
    return client.GetChatClient(options.ChatDeployment);
});

builder.Services.AddSingleton<IRetriever, AzureSearchRetriever>();
builder.Services.AddSingleton<IAnswerGenerator, AzureOpenAIAnswerGenerator>();
builder.Services.AddSingleton<IChatService, ChatService>();

builder.Services.AddOpenApi();

builder.Services.AddCors(cors => cors.AddPolicy(WebCorsPolicy, policy => policy
    .WithOrigins(builder.Configuration.GetSection("Cors:AllowedOrigins").Get<string[]>() ?? [])
    .AllowAnyHeader()
    .AllowAnyMethod()));

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    app.UseSwaggerUI(options => options.SwaggerEndpoint("/openapi/v1.json", "PolicyAssistant API"));
}

app.UseCors(WebCorsPolicy);

app.MapGet("/health", () => Results.Ok(new { status = "healthy" }));

app.MapPost("/chat", async (ChatRequest request, IChatService chatService, CancellationToken cancellationToken) =>
{
    if (string.IsNullOrWhiteSpace(request.Question))
    {
        return Results.ValidationProblem(new Dictionary<string, string[]>
        {
            [nameof(ChatRequest.Question)] = ["Question is required."],
        });
    }

    if (request.Question.Length > MaxQuestionLength)
    {
        return Results.ValidationProblem(new Dictionary<string, string[]>
        {
            [nameof(ChatRequest.Question)] = [$"Question must be at most {MaxQuestionLength} characters."],
        });
    }

    return Results.Ok(await chatService.AskAsync(request, cancellationToken));
})
.Produces<ChatResponse>()
.ProducesValidationProblem();

app.Run();

// Exposed so tests can use WebApplicationFactory<Program>.
public partial class Program;
