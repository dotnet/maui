using Microsoft.Maui.Essentials.AI;
using NaturalLanguage;

namespace Microsoft.Extensions.AI;

/// <summary>
/// Provides extension methods for converting Apple's NaturalLanguage embedding types
/// to <see cref="IEmbeddingGenerator{TInput, TEmbedding}"/> implementations.
/// </summary>
public static class NLEmbeddingExtensions
{
	/// <summary>
	/// Wraps an existing <see cref="NLEmbedding"/> instance as an
	/// <see cref="IEmbeddingGenerator{TInput, TEmbedding}"/> for use with Microsoft.Extensions.AI abstractions.
	/// </summary>
	/// <param name="embedding">The <see cref="NLEmbedding"/> instance to wrap.</param>
	/// <returns>
	/// An <see cref="IEmbeddingGenerator{TInput, TEmbedding}"/> that generates embeddings using the provided
	/// <see cref="NLEmbedding"/>. The returned generator does not own the underlying embedding and will not
	/// dispose it when the generator is disposed.
	/// </returns>
	/// <exception cref="ArgumentNullException">Thrown when <paramref name="embedding"/> is <see langword="null"/>.</exception>
	public static IEmbeddingGenerator<string, Embedding<float>> AsEmbeddingGenerator(this NLEmbedding embedding)
	{
		return new NLEmbeddingGenerator(embedding);
	}
}
