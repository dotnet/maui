using Microsoft.Maui.Essentials.AI;
using NaturalLanguage;

namespace Microsoft.Extensions.AI;

public static class NLEmbeddingExtensions
{
    public static IEmbeddingGenerator<string, Embedding<float>> AsEmbeddingGenerator(this NLEmbedding embedding)
    {
        return new NLEmbeddingGenerator(embedding);
    }
}
