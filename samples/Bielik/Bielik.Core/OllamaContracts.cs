using System.Text.Json.Serialization;

namespace Bielik.Core;

public sealed record ChatMessage(string Role, string Content);

public sealed record GenerationMetrics(int Tokens, long DurationNanoseconds)
{
    public double TokensPerSecond => DurationNanoseconds > 0 ? Tokens * 1_000_000_000d / DurationNanoseconds : 0;
}

public sealed record ChatChunk(string Text, bool IsComplete, GenerationMetrics? Metrics);

internal sealed record ChatRequest(string Model, ChatMessage[] Messages, bool Stream, ChatOptions Options, string KeepAlive);
internal sealed record ChatOptions(double Temperature, int NumCtx, int NumPredict, int NumGpu);
internal sealed record OllamaModel(string? Name, string? Digest, long Size);
internal sealed record ModelsResponse(OllamaModel[]? Models);
internal sealed record StreamResponse(
    string? Model,
    ChatMessage? Message,
    bool Done,
    string? Error,
    int EvalCount,
    long EvalDuration);

[JsonSourceGenerationOptions(PropertyNamingPolicy = JsonKnownNamingPolicy.SnakeCaseLower)]
[JsonSerializable(typeof(ChatRequest))]
[JsonSerializable(typeof(ModelsResponse))]
[JsonSerializable(typeof(StreamResponse))]
internal sealed partial class OllamaJsonContext : JsonSerializerContext;
