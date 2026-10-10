using System.Text.RegularExpressions;
using Microsoft.Extensions.Options;
using OpenAI.Chat;
using PolicyAssistant.Api.Options;
using PolicyAssistant.Api.Retrieval;

namespace PolicyAssistant.Api.Generation;

/// <summary>Generates answers with a Foundry chat deployment through the OpenAI-compatible v1 endpoint.</summary>
public sealed partial class AzureOpenAIAnswerGenerator(
    ChatClient chatClient,
    IOptions<AzureOpenAIOptions> options,
    ILogger<AzureOpenAIAnswerGenerator> logger) : IAnswerGenerator
{
    private readonly AzureOpenAIOptions _options = options.Value;

    public async Task<string> GenerateAsync(string question, IReadOnlyList<RetrievedChunk> sources, CancellationToken cancellationToken)
    {
        ChatMessage[] messages =
        [
            new SystemChatMessage(Prompts.System),
            new UserChatMessage(Prompts.BuildUserMessage(question, sources)),
        ];

        var completionOptions = new ChatCompletionOptions
        {
            Temperature = _options.Temperature,
            MaxOutputTokenCount = _options.MaxOutputTokens,
        };

        ChatCompletion completion = await chatClient.CompleteChatAsync(messages, completionOptions, cancellationToken);

        logger.LogInformation(
            "Chat completion finished ({FinishReason}). Tokens: {InputTokens} in, {OutputTokens} out",
            completion.FinishReason,
            completion.Usage?.InputTokenCount,
            completion.Usage?.OutputTokenCount);

        var text = string.Concat(completion.Content.Select(part => part.Text));
        return StripReasoning(text).Trim();
    }

    // Some models (e.g. DeepSeek reasoning variants) put their reasoning in <think> tags before the answer.
    private static string StripReasoning(string text) => ThinkBlock().Replace(text, string.Empty);

    [GeneratedRegex(@"<think>.*?</think>", RegexOptions.Singleline)]
    private static partial Regex ThinkBlock();
}
