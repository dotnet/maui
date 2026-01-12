using System.Runtime.Versioning;
using Microsoft.Extensions.AI;
using NaturalLanguage;

namespace Microsoft.Maui.Essentials.AI;

/// <summary>
/// Provides an <see cref="IEmbeddingGenerator{TInput, TEmbedding}"/> implementation using Apple's
/// Natural Language framework for generating contextual embeddings.
/// </summary>
/// <remarks>
/// <para>
/// This generator uses Apple's <see cref="NLContextualEmbedding"/> API to generate context-aware
/// vector embeddings for text. Unlike sentence embeddings, contextual embeddings take into account
/// the surrounding context of each token to produce more semantically rich representations.
/// </para>
/// <para>
/// The underlying <see cref="NLContextualEmbedding"/> is not thread-safe, so this class uses a
/// semaphore to ensure that only one embedding generation request is processed at a time. Concurrent
/// calls to <see cref="GenerateAsync"/> will be serialized and processed sequentially.
/// </para>
/// <para>
/// For high-throughput scenarios requiring parallel embedding generation, consider creating multiple
/// instances of this class, each with its own underlying <see cref="NLContextualEmbedding"/>.
/// </para>
/// <para>
/// On first use, this generator will automatically request and load the required model assets.
/// This may take additional time on the first call to <see cref="GenerateAsync"/>.
/// </para>
/// </remarks>
[SupportedOSPlatform("tvos17.0")]
[SupportedOSPlatform("macos14.0")]
[SupportedOSPlatform("ios17.0")]
[SupportedOSPlatform("maccatalyst17.0")]
public class NLContextualEmbeddingGenerator : IEmbeddingGenerator<string, Embedding<float>>
{
    /// <summary>Semaphore to ensure thread-safe access to the embedding generator.</summary>
    private readonly SemaphoreSlim _semaphore = new(1, 1);

    /// <summary>The underlying <see cref="NLContextualEmbedding" />.</summary>
    private readonly NLContextualEmbedding _embedding;

    /// <summary>Whether this instance owns the <see cref="NLContextualEmbedding"/> and is responsible for disposing it.</summary>
    private readonly bool _ownsEmbedding;

    /// <summary>Metadata about the embedding generator.</summary>
    private EmbeddingGeneratorMetadata? _metadata;

    /// <summary>Checks if the assets have been loaded.</summary>
    private bool _assetsLoaded;

    /// <summary>
    /// Initializes a new instance of the <see cref="NLContextualEmbeddingGenerator"/> class
    /// using the English language.
    /// </summary>
    public NLContextualEmbeddingGenerator()
        : this(NLLanguage.English)
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="NLContextualEmbeddingGenerator"/> class
    /// with the specified language.
    /// </summary>
    /// <param name="language">The language for which to generate embeddings.</param>
    public NLContextualEmbeddingGenerator(NLLanguage language)
    {
        _embedding = NLContextualEmbedding.CreateWithLanguage(language.GetConstant()!)
            ?? throw new NotSupportedException($"Contextual embedding for language '{language}' is not supported.");
        _ownsEmbedding = true;
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="NLContextualEmbeddingGenerator"/> class
    /// with the specified <see cref="NLContextualEmbedding"/>.
    /// </summary>
    /// <param name="embedding">The <see cref="NLContextualEmbedding"/> to use for generating embeddings.</param>
    public NLContextualEmbeddingGenerator(NLContextualEmbedding embedding)
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
            if (!_assetsLoaded)
            {
                await _embedding.RequestAssetsAsync().ConfigureAwait(false);
                _assetsLoaded = true;
            }

            foreach (var value in values)
            {
                cancellationToken.ThrowIfCancellationRequested();

                var embeddingResult = _embedding.GetEmbeddingResult(value, null, out var error);

                if (embeddingResult is null)
                {
                    // Return an empty embedding to keep counts aligned, implies failure.
                    result.Add(new Embedding<float>(Array.Empty<float>()));
                    continue;
                }

                float[]? vectors = null;
                int tokenCount = 0;

                embeddingResult.EnumerateTokenVectors(new NSRange(0, (nint)embeddingResult.SequenceLength), (vector, range, out stop) =>
                {
                    var dimension = (nint)vector.Count;

                    vectors ??= new float[dimension];

                    for (var i = 0; i < dimension; i++)
                    {
                        vectors[i] += vector[i].FloatValue;
                    }

                    tokenCount++;

                    stop = cancellationToken.IsCancellationRequested;
                });

                if (vectors is not null && tokenCount > 0)
                {
                    for (var i = 0; i < vectors.Length; i++)
                    {
                        vectors[i] /= tokenCount;
                    }

                    result.Add(new Embedding<float>(vectors));
                }
                else
                {
                    result.Add(new Embedding<float>(Array.Empty<float>()));
                }
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
                defaultModelId: "natural-language-contextual");
        }

        if (serviceType == typeof(NLContextualEmbedding))
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
