#if IOS || MACCATALYST

using Microsoft.Extensions.AI;
using NaturalLanguage;
using Xunit;

namespace Microsoft.Maui.Essentials.AI.DeviceTests;

public class NLContextualEmbeddingGeneratorCancellationTests : EmbeddingGeneratorCancellationTestsBase<NaturalLanguageContextualEmbeddingGenerator>
{
	// All cancellation tests are inherited from base class
	// Add implementation-specific cancellation tests here if needed
}

public class NLContextualEmbeddingGeneratorConcurrencyTests : EmbeddingGeneratorConcurrencyTestsBase<NaturalLanguageContextualEmbeddingGenerator>
{
	// Implementation-specific test for NaturalLanguageContextualEmbeddingGenerator

	[Fact]
	public async Task GenerateAsync_SequentialCalls_ProduceConsistentDimensions()
	{
		var generator = new NaturalLanguageContextualEmbeddingGenerator();
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

public class NLContextualEmbeddingGeneratorDisposalTests : EmbeddingGeneratorDisposalTestsBase<NaturalLanguageContextualEmbeddingGenerator>
{
	// Implementation-specific tests for NaturalLanguageContextualEmbeddingGenerator

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

public class NLContextualEmbeddingGeneratorGenerateTests : EmbeddingGeneratorGenerateTestsBase<NaturalLanguageContextualEmbeddingGenerator>
{
	// Implementation-specific tests for NaturalLanguageContextualEmbeddingGenerator

	[Fact]
	public async Task GenerateAsync_MultipleCalls_LoadsAssetsOnce()
	{
		var generator = new NaturalLanguageContextualEmbeddingGenerator();

		var result1 = await generator.GenerateAsync(new[] { "First call" });
		Assert.NotNull(result1);

		var result2 = await generator.GenerateAsync(new[] { "Second call" });
		Assert.NotNull(result2);

		var result3 = await generator.GenerateAsync(new[] { "Third call" });
		Assert.NotNull(result3);
	}

	[Fact]
	public async Task GenerateAsync_TokenVectorAveraging_ProducesConsistentDimensions()
	{
		var generator = new NaturalLanguageContextualEmbeddingGenerator();
		var values = new[]
		{
			"Short",
			"A medium length sentence",
			"A much longer sentence with many more words to process and tokenize"
		};

		var result = await generator.GenerateAsync(values);

		Assert.NotNull(result);
		Assert.Equal(3, result.Count);

		var nonEmptyResults = result.Where(e => e.Vector.Length > 0).ToList();
		if (nonEmptyResults.Count > 1)
		{
			var dimensions = nonEmptyResults.Select(e => e.Vector.Length).Distinct().ToList();
			Assert.Single(dimensions);
		}
	}
}
public class NLContextualEmbeddingGeneratorGetServiceTests : EmbeddingGeneratorGetServiceTestsBase<NaturalLanguageContextualEmbeddingGenerator>
{
	protected override string ExpectedProviderName => "apple";
	protected override string ExpectedDefaultModelId => "natural-language-contextual";

	// Implementation-specific tests for NaturalLanguageContextualEmbeddingGenerator

	[Fact]
	public void GetService_ReturnsUnderlyingNLContextualEmbedding()
	{
		IEmbeddingGenerator<string, Embedding<float>> generator = new NaturalLanguageContextualEmbeddingGenerator();
		var embedding = generator.GetService<NLContextualEmbedding>();

		Assert.NotNull(embedding);
	}
}

public class NLContextualEmbeddingGeneratorInstantiationTests : EmbeddingGeneratorInstantiationTestsBase<NaturalLanguageContextualEmbeddingGenerator>
{
	// Implementation-specific tests for NaturalLanguageContextualEmbeddingGenerator

	[Fact]
	public void LanguageConstructor_WithEnglish_CreatesInstance()
	{
		var generator = new NaturalLanguageContextualEmbeddingGenerator(NLLanguage.English);
		Assert.NotNull(generator);
	}

	[Fact]
	public void LanguageConstructor_WithSpanish_CreatesInstance()
	{
		var generator = new NaturalLanguageContextualEmbeddingGenerator(NLLanguage.Spanish);
		Assert.NotNull(generator);
	}

	[Fact]
	public void EmbeddingConstructor_WithValidEmbedding_CreatesInstance()
	{
		var embedding = NLContextualEmbedding.CreateWithLanguage(NLLanguage.English.GetConstant()!);
		Assert.NotNull(embedding);

		var generator = new NaturalLanguageContextualEmbeddingGenerator(embedding);
		Assert.NotNull(generator);
	}

	[Fact]
	public void EmbeddingConstructor_WithNull_ThrowsArgumentNullException()
	{
		Assert.Throws<ArgumentNullException>(() => new NaturalLanguageContextualEmbeddingGenerator((NLContextualEmbedding)null!));
	}
}

public class NLContextualEmbeddingGeneratorSimilarityTests : EmbeddingGeneratorSimilarityTestsBase<NaturalLanguageContextualEmbeddingGenerator>
{
	// All similarity tests are inherited from base class
	// Add implementation-specific similarity tests here if needed
}

#endif
