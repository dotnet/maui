using Microsoft.Maui.Essentials.AI;
using NaturalLanguage;

namespace Microsoft.Extensions.AI;

public static class NLContextualEmbeddingExtensions
{
	public static IEmbeddingGenerator<string, Embedding<float>> AsEmbeddingGenerator(this NLContextualEmbedding embedding)
	{
		return new NLContextualEmbeddingGenerator(embedding);
	}
}
