#if IOS || MACCATALYST

using Microsoft.Extensions.AI;
using NaturalLanguage;
using Xunit;

namespace Microsoft.Maui.Essentials.AI.DeviceTests;

public class NLEmbeddingGeneratorCancellationTests : EmbeddingGeneratorCancellationTestsBase<NaturalLanguageEmbeddingGenerator>
{
	// All cancellation tests are inherited from base class
	// Add implementation-specific cancellation tests here if needed
}

public class NLEmbeddingGeneratorConcurrencyTests : EmbeddingGeneratorConcurrencyTestsBase<NaturalLanguageEmbeddingGenerator>
{
	// All concurrency tests are inherited from base class
	// Add implementation-specific concurrency tests here if needed
}

public class NLEmbeddingGeneratorDisposalTests : EmbeddingGeneratorDisposalTestsBase<NaturalLanguageEmbeddingGenerator>
{
	// Implementation-specific tests for NaturalLanguageEmbeddingGenerator

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
public class NLEmbeddingGeneratorGenerateTests : EmbeddingGeneratorGenerateTestsBase<NaturalLanguageEmbeddingGenerator>
{
	// Implementation-specific tests for NaturalLanguageEmbeddingGenerator

	[Fact]
	public async Task GenerateAsync_WithSingleValue_ReturnsNonEmptyVector()
	{
		var generator = new NaturalLanguageEmbeddingGenerator();
		var values = new[] { "Hello world" };

		var result = await generator.GenerateAsync(values);

		Assert.NotNull(result);
		Assert.Single(result);
		Assert.NotEmpty(result[0].Vector.ToArray());
	}

	[Fact]
	public async Task GenerateAsync_WithMultipleValues_ReturnsNonEmptyVectors()
	{
		var generator = new NaturalLanguageEmbeddingGenerator();
		var values = new[] { "Hello", "World", "Test" };

		var result = await generator.GenerateAsync(values);

		Assert.NotNull(result);
		Assert.Equal(3, result.Count);
		foreach (var embedding in result)
		{
			Assert.NotEmpty(embedding.Vector.ToArray());
		}
	}
}
public class NLEmbeddingGeneratorGetServiceTests : EmbeddingGeneratorGetServiceTestsBase<NaturalLanguageEmbeddingGenerator>
{
	protected override string ExpectedProviderName => "apple";
	protected override string ExpectedDefaultModelId => "natural-language";

	// Implementation-specific tests for NaturalLanguageEmbeddingGenerator

	[Fact]
	public void GetService_ReturnsUnderlyingNLEmbedding()
	{
		IEmbeddingGenerator<string, Embedding<float>> generator = new NaturalLanguageEmbeddingGenerator();
		var embedding = generator.GetService<NLEmbedding>();

		Assert.NotNull(embedding);
	}
}
public class NLEmbeddingGeneratorInstantiationTests : EmbeddingGeneratorInstantiationTestsBase<NaturalLanguageEmbeddingGenerator>
{
	// Implementation-specific tests for NaturalLanguageEmbeddingGenerator

	[Fact]
	public void LanguageConstructor_WithEnglish_CreatesInstance()
	{
		var generator = new NaturalLanguageEmbeddingGenerator(NLLanguage.English);
		Assert.NotNull(generator);
	}

	[Fact]
	public void LanguageConstructor_WithSpanish_CreatesInstance()
	{
		var generator = new NaturalLanguageEmbeddingGenerator(NLLanguage.Spanish);
		Assert.NotNull(generator);
	}

	[Fact]
	public void EmbeddingConstructor_WithValidEmbedding_CreatesInstance()
	{
		var embedding = NLEmbedding.GetSentenceEmbedding(NLLanguage.English);
		Assert.NotNull(embedding);

		var generator = new NaturalLanguageEmbeddingGenerator(embedding);
		Assert.NotNull(generator);
	}

	[Fact]
	public void EmbeddingConstructor_WithNull_ThrowsArgumentNullException()
	{
		Assert.Throws<ArgumentNullException>(() => new NaturalLanguageEmbeddingGenerator((NLEmbedding)null!));
	}
}

public class NLEmbeddingGeneratorSimilarityTests : EmbeddingGeneratorSimilarityTestsBase<NaturalLanguageEmbeddingGenerator>
{
	// All similarity tests are inherited from base class
	// Add implementation-specific similarity tests here if needed
}

#endif
