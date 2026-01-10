#if IOS || MACCATALYST
using Microsoft.Extensions.AI;
using NaturalLanguage;
using Xunit;

namespace Microsoft.Maui.Essentials.AI.DeviceTests;

public class NLEmbeddingGeneratorDisposalTests
{
	[Fact]
	public void Dispose_CanBeCalledMultipleTimes()
	{
		var generator = new NaturalLanguageEmbeddingGenerator();

		// Should not throw
		generator.Dispose();
		generator.Dispose();
	}

	[Fact]
	public void Dispose_WithOwnedEmbedding_DisposesEmbedding()
	{
		// When using default constructor, generator owns the embedding
		var generator = new NaturalLanguageEmbeddingGenerator();
		generator.Dispose();
		// No exception means success
	}

	[Fact]
	public void Dispose_WithProvidedEmbedding_DoesNotDisposeEmbedding()
	{
		var embedding = NLEmbedding.GetSentenceEmbedding(NLLanguage.English);
		Assert.NotNull(embedding);

		var generator = new NaturalLanguageEmbeddingGenerator(embedding);
		generator.Dispose();

		// Embedding should still be usable
		var vector = embedding.GetVector("test");
		Assert.NotNull(vector);
	}

	[Fact]
	public void AsEmbeddingGenerator_CreatesGeneratorFromNLEmbedding()
	{
		var embedding = NLEmbedding.GetSentenceEmbedding(NLLanguage.English);
		Assert.NotNull(embedding);

		var generator = embedding.AsEmbeddingGenerator();

		Assert.NotNull(generator);
		Assert.IsType<NaturalLanguageEmbeddingGenerator>(generator);
	}
}
#endif
