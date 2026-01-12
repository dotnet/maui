using System.Runtime.Versioning;
using Microsoft.Extensions.AI;
using NaturalLanguage;

namespace Microsoft.Maui.Essentials.AI;

/// <summary>
/// Provides an <see cref="IEmbeddingGenerator{TInput, TEmbedding}"/> implementation using Apple's
/// Natural Language framework for generating sentence embeddings.
/// </summary>
/// <remarks>
/// <para>
/// This generator uses Apple's <see cref="NLEmbedding"/> API to generate vector embeddings for text.
/// The underlying <see cref="NLEmbedding"/> is not thread-safe, so this class uses a semaphore to
/// ensure that only one embedding generation request is processed at a time. Concurrent calls to
/// <see cref="GenerateAsync"/> will be serialized and processed sequentially.
/// </para>
/// <para>
/// For high-throughput scenarios requiring parallel embedding generation, consider creating multiple
/// instances of this class, each with its own underlying <see cref="NLEmbedding"/>.
/// </para>
/// </remarks>
[SupportedOSPlatform("tvos13.0")]
[SupportedOSPlatform("ios13.0")]
[SupportedOSPlatform("maccatalyst")]
[SupportedOSPlatform("macos")]
public class NLEmbeddingGenerator : IEmbeddingGenerator<string, Embedding<float>>
{
    /// <summary>Semaphore to ensure thread-safe access to the embedding generator.</summary>
    private readonly SemaphoreSlim _semaphore = new(1, 1);

    /// <summary>The underlying <see cref="NLEmbedding" />.</summary>
    private readonly NLEmbedding _embedding;

    /// <summary>Whether this instance owns the <see cref="NLEmbedding"/> and is responsible for disposing it.</summary>
    private readonly bool _ownsEmbedding;

    /// <summary>Metadata about the embedding generator.</summary>
    private EmbeddingGeneratorMetadata? _metadata;

    /// <summary>
    /// Initializes a new instance of the <see cref="NLEmbeddingGenerator"/> class
    /// using the English sentence embedding.
    /// </summary>
    public NLEmbeddingGenerator()
        : this(NLLanguage.English)
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="NLEmbeddingGenerator"/> class
    /// with the specified language's sentence embedding.
    /// </summary>
    /// <param name="language">The language for which to generate embeddings.</param>
    public NLEmbeddingGenerator(NLLanguage language)
    {
        _embedding = NLEmbedding.GetSentenceEmbedding(language)
            ?? throw new NotSupportedException($"Sentence embedding for language '{language}' is not supported.");
        _ownsEmbedding = true;
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="NLEmbeddingGenerator"/> class
    /// with the specified <see cref="NLEmbedding"/>.
    /// </summary>
    /// <param name="embedding">The <see cref="NLEmbedding"/> to use for generating embeddings.</param>
    public NLEmbeddingGenerator(NLEmbedding embedding)
    {
        _embedding = embedding ?? throw new ArgumentNullException(nameof(embedding));
        _ownsEmbedding = false;
    }

    /// <inheritdoc />
    public async Task<GeneratedEmbeddings<Embedding<float>>> GenerateAsync(
        IEnumerable<string> values,
        EmbeddingGenerationOptions? options = null,
        CancellationToken cancellationToken = default)
    {
        var potentialCount = values.TryGetNonEnumeratedCount(out var count) ? count : 0;

        var result = new GeneratedEmbeddings<Embedding<float>>(potentialCount);

        await _semaphore.WaitAsync(cancellationToken).ConfigureAwait(false);
        try
        {
            foreach (var value in values)
            {
                cancellationToken.ThrowIfCancellationRequested();

                var vector = _embedding.GetVector(value);

                result.Add(new Embedding<float>(vector ?? []));
            }
        }
        finally
        {
            _semaphore.Release();
        }

        return result;
    }

    /// <inheritdoc />
    object? IEmbeddingGenerator.GetService(Type serviceType, object? serviceKey)
    {
        if (serviceType is null)
        {
            throw new ArgumentNullException(nameof(serviceType));
        }

        if (serviceKey is not null)
        {
            return null;
        }

        if (serviceType == typeof(EmbeddingGeneratorMetadata))
        {
            return _metadata ??= new EmbeddingGeneratorMetadata(
                providerName: "apple",
                defaultModelId: "natural-language");
        }

        if (serviceType == typeof(NLEmbedding))
        {
            return _embedding;
        }

        if (serviceType.IsInstanceOfType(this))
        {
            return this;
        }

        return null;
    }

    /// <inheritdoc />
    public void Dispose()
    {
        if (_ownsEmbedding)
        {
            _embedding.Dispose();
        }
        _semaphore.Dispose();
    }
}
