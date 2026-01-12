#if IOS || MACCATALYST

using Microsoft.Extensions.AI;
using NaturalLanguage;
using Xunit;

namespace Microsoft.Maui.Essentials.AI.DeviceTests;

[Category("NLContextualEmbeddingGenerator")]
public class NLContextualEmbeddingGeneratorCancellationTests : EmbeddingGeneratorCancellationTestsBase<NLContextualEmbeddingGenerator>
{
}

[Category("NLContextualEmbeddingGenerator")]
public class NLContextualEmbeddingGeneratorConcurrencyTests : EmbeddingGeneratorConcurrencyTestsBase<NLContextualEmbeddingGenerator>
{
	[Fact]
	public async Task GenerateAsync_SequentialCalls_ProduceConsistentDimensions()
	{
		var generator = new NLContextualEmbeddingGenerator();
		var testText = "The quick brown fox";

		var result1 = await generator.GenerateAsync(new[] { testText });
		var result2 = await generator.GenerateAsync(new[] { testText });

		Assert.NotNull(result1);
		Assert.NotNull(result2);
		Assert.Single(result1);
		Assert.Single(result2);

		// Same text should produce same embedding dimensions
		var vec1 = result1[0].Vector.ToArray();
		var vec2 = result2[0].Vector.ToArray();

		Assert.Equal(vec1.Length, vec2.Length);
	}
}

[Category("NLContextualEmbeddingGenerator")]
public class NLContextualEmbeddingGeneratorDisposalTests : EmbeddingGeneratorDisposalTestsBase<NLContextualEmbeddingGenerator>
{
	[Fact]
	public void Dispose_WithOwnedEmbedding_DisposesEmbedding()
	{
		// When using default constructor, generator owns the embedding
		var generator = new NLContextualEmbeddingGenerator();
		generator.Dispose();
		// No exception means success
	}

	[Fact]
	public void Dispose_WithProvidedEmbedding_DoesNotDisposeEmbedding()
	{
		var embedding = NLContextualEmbedding.CreateWithLanguage(NLLanguage.English.GetConstant()!);
		Assert.NotNull(embedding);

		var generator = new NLContextualEmbeddingGenerator(embedding);
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
		Assert.IsType<NLContextualEmbeddingGenerator>(generator);
	}
}

[Category("NLContextualEmbeddingGenerator")]
public class NLContextualEmbeddingGeneratorGenerateTests : EmbeddingGeneratorGenerateTestsBase<NLContextualEmbeddingGenerator>
{
}

[Category("NLContextualEmbeddingGenerator")]
public class NLContextualEmbeddingGeneratorGetServiceTests : EmbeddingGeneratorGetServiceTestsBase<NLContextualEmbeddingGenerator>
{
	protected override string ExpectedProviderName => "apple";
	protected override string ExpectedDefaultModelId => "natural-language-contextual";

	[Fact]
	public void GetService_ReturnsUnderlyingNLContextualEmbedding()
	{
		IEmbeddingGenerator<string, Embedding<float>> generator = new NLContextualEmbeddingGenerator();
		var embedding = generator.GetService<NLContextualEmbedding>();

		Assert.NotNull(embedding);
	}
}

[Category("NLContextualEmbeddingGenerator")]
public class NLContextualEmbeddingGeneratorInstantiationTests : EmbeddingGeneratorInstantiationTestsBase<NLContextualEmbeddingGenerator>
{
	[Fact]
	public void LanguageConstructor_WithEnglish_CreatesInstance()
	{
		var generator = new NLContextualEmbeddingGenerator(NLLanguage.English);
		Assert.NotNull(generator);
	}

	[Fact]
	public void LanguageConstructor_WithSpanish_CreatesInstance()
	{
		var generator = new NLContextualEmbeddingGenerator(NLLanguage.Spanish);
		Assert.NotNull(generator);
	}

	[Fact]
	public void EmbeddingConstructor_WithValidEmbedding_CreatesInstance()
	{
		var embedding = NLContextualEmbedding.CreateWithLanguage(NLLanguage.English.GetConstant()!);
		Assert.NotNull(embedding);

		var generator = new NLContextualEmbeddingGenerator(embedding);
		Assert.NotNull(generator);
	}

	[Fact]
	public void EmbeddingConstructor_WithNull_ThrowsArgumentNullException()
	{
		Assert.Throws<ArgumentNullException>(() => new NLContextualEmbeddingGenerator((NLContextualEmbedding)null!));
	}
}

[Category("NLContextualEmbeddingGenerator")]
public class NLContextualEmbeddingGeneratorSimilarityTests : EmbeddingGeneratorSimilarityTestsBase<NLContextualEmbeddingGenerator>
{
}

#endif
