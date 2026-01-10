#if IOS || MACCATALYST
using Microsoft.Extensions.AI;
using NaturalLanguage;
using Xunit;

namespace Microsoft.Maui.Essentials.AI.DeviceTests;

public class NLContextualEmbeddingGeneratorDisposalTests
{
	[Fact]
	public void Dispose_CanBeCalledMultipleTimes()
	{
		var generator = new NaturalLanguageContextualEmbeddingGenerator();

		// Should not throw
		generator.Dispose();
		generator.Dispose();
	}

	[Fact]
	public void Dispose_WithOwnedEmbedding_DisposesEmbedding()
	{
		// When using default constructor, generator owns the embedding
		var generator = new NaturalLanguageContextualEmbeddingGenerator();
		generator.Dispose();
		// No exception means success
	}

	[Fact]
	public void Dispose_WithProvidedEmbedding_DoesNotDisposeEmbedding()
	{
		var embedding = NLContextualEmbedding.CreateWithLanguage(NLLanguage.English.GetConstant()!);
		Assert.NotNull(embedding);

		var generator = new NaturalLanguageContextualEmbeddingGenerator(embedding);
		generator.Dispose();

		// Embedding should still be accessible (not disposed)
		Assert.NotNull(embedding.Languages);
	}

	[Fact]
	public void AsEmbeddingGenerator_CreatesGeneratorFromNLContextualEmbedding()
	{
		var embedding = NLContextualEmbedding.CreateWithLanguage(NLLanguage.English.GetConstant()!);
		Assert.NotNull(embedding);

		var generator = embedding.AsEmbeddingGenerator();

		Assert.NotNull(generator);
		Assert.IsType<NaturalLanguageContextualEmbeddingGenerator>(generator);
	}
}
#endif
