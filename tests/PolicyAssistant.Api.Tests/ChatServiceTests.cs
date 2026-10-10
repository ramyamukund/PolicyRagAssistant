using Microsoft.Extensions.Logging.Abstractions;
using NSubstitute;
using PolicyAssistant.Api.Chat;
using PolicyAssistant.Api.Generation;
using PolicyAssistant.Api.Retrieval;
using PolicyAssistant.Shared;

namespace PolicyAssistant.Api.Tests;

public class ChatServiceTests
{
    private readonly IRetriever _retriever = Substitute.For<IRetriever>();
    private readonly IAnswerGenerator _generator = Substitute.For<IAnswerGenerator>();
    private readonly ChatService _service;

    public ChatServiceTests()
    {
        _service = new ChatService(_retriever, _generator, NullLogger<ChatService>.Instance);
    }

    [Fact]
    public async Task NoRetrievedChunks_ReturnsNotFound_WithoutCallingGenerator()
    {
        GivenChunks();

        var response = await _service.AskAsync(new ChatRequest("What is the dress code?"), CancellationToken.None);

        Assert.Equal(Prompts.NotFoundAnswer, response.Answer);
        Assert.Empty(response.Citations);
        Assert.False(response.IsGrounded);
        await _generator.DidNotReceiveWithAnyArgs().GenerateAsync(default!, default!, default);
    }

    [Fact]
    public async Task AnswerCitingOneAndThree_ReturnsExactlyThoseCitations()
    {
        GivenChunks(Chunk("a"), Chunk("b"), Chunk("c"));
        GivenAnswer("You get 25 days of leave [1]. Carry-over is capped at 5 days [3].");

        var response = await _service.AskAsync(new ChatRequest("How much leave do I get?"), CancellationToken.None);

        Assert.Equal([1, 3], response.Citations.Select(c => c.Number));
        Assert.Equal(["a", "c"], response.Citations.Select(c => c.ChunkId));
        Assert.True(response.IsGrounded);
    }

    [Fact]
    public async Task AnswerWithoutCitationMarkers_IsNotGrounded()
    {
        GivenChunks(Chunk("a"));
        GivenAnswer("You get 25 days of leave.");

        var response = await _service.AskAsync(new ChatRequest("How much leave do I get?"), CancellationToken.None);

        Assert.Empty(response.Citations);
        Assert.False(response.IsGrounded);
    }

    [Fact]
    public async Task CommaSeparatedAndRepeatedMarkers_AreDeduplicated_AndOutOfRangeIgnored()
    {
        GivenChunks(Chunk("a"), Chunk("b"));
        GivenAnswer("Yes [2, 1]. See also [1] and [7].");

        var response = await _service.AskAsync(new ChatRequest("Can I work remotely?"), CancellationToken.None);

        Assert.Equal([1, 2], response.Citations.Select(c => c.Number));
    }

    private void GivenChunks(params RetrievedChunk[] chunks) =>
        _retriever.RetrieveAsync(Arg.Any<string>(), Arg.Any<CancellationToken>()).Returns(chunks);

    private void GivenAnswer(string answer) =>
        _generator.GenerateAsync(Arg.Any<string>(), Arg.Any<IReadOnlyList<RetrievedChunk>>(), Arg.Any<CancellationToken>())
            .Returns(answer);

    private static RetrievedChunk Chunk(string id) => new(id, $"Policy {id}", $"Content of {id}", RerankerScore: 3.0);
}
