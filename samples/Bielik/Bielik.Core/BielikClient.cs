using System.Net.Http.Json;
using System.Runtime.CompilerServices;
using System.Text.Json;

namespace Bielik.Core;

public sealed class BielikClient(HttpClient httpClient)
{
    public static HttpClient CreateLocalHttpClient() => new(new SocketsHttpHandler
    {
        AllowAutoRedirect = false,
        UseProxy = false,
        ConnectTimeout = TimeSpan.FromSeconds(8)
    })
    {
        Timeout = Timeout.InfiniteTimeSpan
    };

    public async Task<string> CheckConnectionAsync(LocalEndpoint endpoint, CancellationToken cancellationToken = default)
    {
        using var timeout = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
        timeout.CancelAfter(TimeSpan.FromSeconds(10));
        using var response = await httpClient.GetAsync(new Uri(endpoint.Address, "api/tags"), timeout.Token);
        response.EnsureSuccessStatusCode();
        var models = await response.Content.ReadFromJsonAsync(OllamaJsonContext.Default.ModelsResponse, timeout.Token)
            ?? throw new InvalidDataException("Serwer nie zwrócił listy modeli.");
        var model = models.Models?.FirstOrDefault(item => string.Equals(item.Name, ModelInfo.Id, StringComparison.OrdinalIgnoreCase))
            ?? throw new InvalidOperationException($"Na serwerze brakuje Bielika. Uruchom na Macu: {ModelInfo.PullCommand}");
        if (string.IsNullOrWhiteSpace(model.Digest))
        {
            throw new InvalidDataException("Serwer nie zwrócił identyfikatora pobranego modelu.");
        }

        return model.Digest;
    }

    public async IAsyncEnumerable<ChatChunk> StreamAsync(
        LocalEndpoint endpoint,
        IReadOnlyList<ChatMessage> conversation,
        [EnumeratorCancellation] CancellationToken cancellationToken = default)
    {
        if (conversation.Count % 2 != 1 ||
            conversation.Where((message, index) =>
                message.Role != (index % 2 == 0 ? "user" : "assistant") || string.IsNullOrWhiteSpace(message.Content)).Any())
        {
            throw new ArgumentException("Rozmowa musi zawierać pełne pary wiadomości i kończyć się pytaniem użytkownika.", nameof(conversation));
        }

        var messages = new[] { new ChatMessage("system", ModelInfo.SystemPrompt) }
            .Concat(conversation.TakeLast(11))
            .ToArray();
        var requestBody = new ChatRequest(ModelInfo.Id, messages, true, new ChatOptions(0.1, 8192, 768, 0), "15m");

        using var timeout = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
        timeout.CancelAfter(TimeSpan.FromMinutes(3));
        using var request = new HttpRequestMessage(HttpMethod.Post, new Uri(endpoint.Address, "api/chat"))
        {
            Content = JsonContent.Create(requestBody, OllamaJsonContext.Default.ChatRequest)
        };
        using var response = await httpClient.SendAsync(request, HttpCompletionOption.ResponseHeadersRead, timeout.Token);
        response.EnsureSuccessStatusCode();
        await using var stream = await response.Content.ReadAsStreamAsync(timeout.Token);
        using var reader = new StreamReader(stream);
        var hasContent = false;

        while (await reader.ReadLineAsync(timeout.Token) is { } line)
        {
            if (string.IsNullOrWhiteSpace(line))
            {
                continue;
            }

            var chunk = JsonSerializer.Deserialize(line, OllamaJsonContext.Default.StreamResponse)
                ?? throw new InvalidDataException("Serwer zwrócił pusty fragment odpowiedzi.");
            if (!string.IsNullOrEmpty(chunk.Error))
            {
                throw new InvalidOperationException($"Ollama: {chunk.Error}");
            }

            if (!string.Equals(chunk.Model, ModelInfo.Id, StringComparison.OrdinalIgnoreCase))
            {
                throw new InvalidDataException("Serwer odpowiedział innym modelem. Dozwolony jest tylko lokalny Bielik.");
            }

            if (chunk.Message is { Role: not "assistant" })
            {
                throw new InvalidDataException("Serwer zwrócił nieprawidłową rolę wiadomości.");
            }

            var text = chunk.Message?.Content ?? "";
            hasContent |= !string.IsNullOrWhiteSpace(text);
            if (chunk.Done && !hasContent)
            {
                throw new InvalidDataException("Bielik zakończył generowanie bez odpowiedzi. Spróbuj ponownie.");
            }

            yield return new ChatChunk(text, chunk.Done,
                chunk.Done ? new GenerationMetrics(chunk.EvalCount, chunk.EvalDuration) : null);
            if (chunk.Done)
            {
                yield break;
            }
        }

        throw new EndOfStreamException("Połączenie przerwano przed zakończeniem odpowiedzi. Spróbuj ponownie.");
    }
}
