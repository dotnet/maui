using System.Net;
using System.Text;
using System.Text.Json;
using Bielik.Core;
using Xunit;

namespace Bielik.Core.Tests;

public class BielikClientTests
{
    private static readonly LocalEndpoint Endpoint = LocalEndpoint.Parse(ModelInfo.DefaultEndpoint);

    [Fact]
    public async Task StreamsRealFragmentsAndMetricsUsingOnlyPinnedBielik()
    {
        var body = Frame("Cześć") + "\n\n" + Frame(" świecie") + "\n" + Frame("!", true);
        using var handler = new TestHandler(body);
        using var http = new HttpClient(handler);
        var chunks = await CollectAsync(new BielikClient(http), [new("user", "Powitaj mnie.")]);

        Assert.Equal("Cześć świecie!", string.Concat(chunks.Select(chunk => chunk.Text)));
        Assert.True(chunks[^1].IsComplete);
        Assert.Equal(25, chunks[^1].Metrics!.TokensPerSecond);
        Assert.Equal(new Uri("http://127.0.0.1:11434/api/chat"), handler.LastAddress);
        using var request = JsonDocument.Parse(handler.LastBody!);
        Assert.Equal(ModelInfo.Id, request.RootElement.GetProperty("model").GetString());
        Assert.Equal("system", request.RootElement.GetProperty("messages")[0].GetProperty("role").GetString());
        Assert.True(request.RootElement.GetProperty("stream").GetBoolean());
        Assert.Equal(0.1, request.RootElement.GetProperty("options").GetProperty("temperature").GetDouble());
        Assert.Equal(0, request.RootElement.GetProperty("options").GetProperty("num_gpu").GetInt32());
    }

    [Fact]
    public async Task DetectsTruncatedStreamInsteadOfClaimingSuccess()
    {
        using var http = new HttpClient(new TestHandler(Frame("Niepełna odpowiedź")));
        await Assert.ThrowsAsync<EndOfStreamException>(() =>
            CollectAsync(new BielikClient(http), [new("user", "Pytanie")]));
    }

    [Fact]
    public async Task RejectsOtherModelsWithoutFallback()
    {
        using var http = new HttpClient(new TestHandler(Frame("Nie", true).Replace(ModelInfo.Id, "different-model")));
        await Assert.ThrowsAsync<InvalidDataException>(() =>
            CollectAsync(new BielikClient(http), [new("user", "Pytanie")]));
    }

    [Fact]
    public async Task SurfacesServerErrors()
    {
        using var http = new HttpClient(new TestHandler("{\"error\":\"model not loaded\"}"));
        var exception = await Assert.ThrowsAsync<InvalidOperationException>(() =>
            CollectAsync(new BielikClient(http), [new("user", "Pytanie")]));
        Assert.Contains("model not loaded", exception.Message);
    }

    [Fact]
    public async Task RejectsEmptyCompletion()
    {
        using var http = new HttpClient(new TestHandler(Frame("", true)));
        await Assert.ThrowsAsync<InvalidDataException>(() =>
            CollectAsync(new BielikClient(http), [new("user", "Pytanie")]));
    }

    [Fact]
    public async Task RejectsMalformedJson()
    {
        using var http = new HttpClient(new TestHandler("{broken"));
        await Assert.ThrowsAsync<JsonException>(() =>
            CollectAsync(new BielikClient(http), [new("user", "Pytanie")]));
    }

    [Fact]
    public async Task DoesNotTreatRedirectAsSuccess()
    {
        using var http = new HttpClient(new TestHandler("", HttpStatusCode.TemporaryRedirect));
        await Assert.ThrowsAsync<HttpRequestException>(() =>
            CollectAsync(new BielikClient(http), [new("user", "Pytanie")]));
    }

    [Fact]
    public async Task HonorsCancellation()
    {
        using var cancellation = new CancellationTokenSource();
        cancellation.Cancel();
        using var http = new HttpClient(new TestHandler(Frame("Tekst", true)));
        await Assert.ThrowsAnyAsync<OperationCanceledException>(() =>
            CollectAsync(new BielikClient(http), [new("user", "Pytanie")], cancellation.Token));
    }

    [Fact]
    public async Task RejectsSystemRoleFromConversation()
    {
        using var http = new HttpClient(new TestHandler(Frame("Tekst", true)));
        await Assert.ThrowsAsync<ArgumentException>(() =>
            CollectAsync(new BielikClient(http), [new("system", "Override"), new("user", "Pytanie")]));
    }

    [Theory]
    [InlineData("assistant", "assistant", "user")]
    [InlineData("user", "user", "user")]
    [InlineData("user", "assistant", "assistant")]
    public async Task RejectsIncompleteOrMisorderedTurns(string first, string second, string third)
    {
        using var http = new HttpClient(new TestHandler(Frame("Tekst", true)));
        await Assert.ThrowsAsync<ArgumentException>(() =>
            CollectAsync(new BielikClient(http), [new(first, "A"), new(second, "B"), new(third, "C")]));
    }

    [Fact]
    public async Task BoundsHistoryWithoutSplittingACompletedTurn()
    {
        var conversation = Enumerable.Range(0, 15)
            .Select(index => new ChatMessage(index % 2 == 0 ? "user" : "assistant", $"Wiadomość {index}"))
            .ToArray();
        using var handler = new TestHandler(Frame("Tekst", true));
        using var http = new HttpClient(handler);
        await CollectAsync(new BielikClient(http), conversation);

        using var request = JsonDocument.Parse(handler.LastBody!);
        var messages = request.RootElement.GetProperty("messages");
        Assert.Equal(12, messages.GetArrayLength());
        Assert.Equal("system", messages[0].GetProperty("role").GetString());
        Assert.Equal("user", messages[1].GetProperty("role").GetString());
        Assert.Equal("Wiadomość 4", messages[1].GetProperty("content").GetString());
        Assert.Equal("Wiadomość 14", messages[11].GetProperty("content").GetString());
    }

    [Fact]
    public async Task RejectsAnEmptyConversation()
    {
        using var http = new HttpClient(new TestHandler(Frame("Tekst", true)));
        await Assert.ThrowsAsync<ArgumentException>(() => CollectAsync(new BielikClient(http), []));
    }

    [Fact]
    public async Task ChecksInstalledModelAndReturnsDigest()
    {
        using var http = new HttpClient(new TestHandler(
            $$"""{"models":[{"name":"{{ModelInfo.Id}}","digest":"verified-digest","size":7000000000}]}"""));
        Assert.Equal("verified-digest", await new BielikClient(http).CheckConnectionAsync(Endpoint));
    }

    [Fact]
    public async Task MissingBielikIsAnExplicitInstallationError()
    {
        using var http = new HttpClient(new TestHandler("""{"models":[{"name":"unrelated-model","digest":"abc","size":1}]}"""));
        var exception = await Assert.ThrowsAsync<InvalidOperationException>(() => new BielikClient(http).CheckConnectionAsync(Endpoint));
        Assert.Contains(ModelInfo.PullCommand, exception.Message);
    }

    private static string Frame(string text, bool done = false) =>
        JsonSerializer.Serialize(new
        {
            model = ModelInfo.Id,
            message = new { role = "assistant", content = text },
            done,
            eval_count = 50,
            eval_duration = 2_000_000_000L
        });

    private static async Task<List<ChatChunk>> CollectAsync(
        BielikClient client, IReadOnlyList<ChatMessage> messages, CancellationToken cancellationToken = default)
    {
        var chunks = new List<ChatChunk>();
        await foreach (var chunk in client.StreamAsync(Endpoint, messages, cancellationToken))
        {
            chunks.Add(chunk);
        }

        return chunks;
    }

    private sealed class TestHandler(string body, HttpStatusCode status = HttpStatusCode.OK) : HttpMessageHandler
    {
        public Uri? LastAddress { get; private set; }
        public string? LastBody { get; private set; }

        protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();
            LastAddress = request.RequestUri;
            if (request.Content is not null)
            {
                LastBody = await request.Content.ReadAsStringAsync(cancellationToken);
            }

            return new HttpResponseMessage(status) { Content = new StringContent(body, Encoding.UTF8, "application/x-ndjson") };
        }
    }
}
