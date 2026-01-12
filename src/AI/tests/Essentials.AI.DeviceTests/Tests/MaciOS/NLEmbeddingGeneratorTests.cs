#if IOS || MACCATALYST

using Microsoft.Extensions.AI;
using NaturalLanguage;
using Xunit;

namespace Microsoft.Maui.Essentials.AI.DeviceTests;

[Category("NLEmbeddingGenerator")]
public class NLEmbeddingGeneratorCancellationTests : EmbeddingGeneratorCancellationTestsBase<NLEmbeddingGenerator>
{
}

[Category("NLEmbeddingGenerator")]
public class NLEmbeddingGeneratorConcurrencyTests : EmbeddingGeneratorConcurrencyTestsBase<NLEmbeddingGenerator>
{
}

[Category("NLEmbeddingGenerator")]
public class NLEmbeddingGeneratorDisposalTests : EmbeddingGeneratorDisposalTestsBase<NLEmbeddingGenerator>
{
	[Fact]
	public void Dispose_WithOwnedEmbedding_DisposesEmbedding()
	{
		// When using default constructor, generator owns the embedding
		var generator = new NLEmbeddingGenerator();
		generator.Dispose();
		// No exception means success
	}

	[Fact]
	public void Dispose_WithProvidedEmbedding_DoesNotDisposeEmbedding()
	{
		var embedding = NLEmbedding.GetSentenceEmbedding(NLLanguage.English);
		Assert.NotNull(embedding);

		var generator = new NLEmbeddingGenerator(embedding);
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
		Assert.IsType<NLEmbeddingGenerator>(generator);
	}
}

[Category("NLEmbeddingGenerator")]
public class NLEmbeddingGeneratorGenerateTests : EmbeddingGeneratorGenerateTestsBase<NLEmbeddingGenerator>
{
}

[Category("NLEmbeddingGenerator")]
public class NLEmbeddingGeneratorGetServiceTests : EmbeddingGeneratorGetServiceTestsBase<NLEmbeddingGenerator>
{
	protected override string ExpectedProviderName => "apple";
	protected override string ExpectedDefaultModelId => "natural-language";

	[Fact]
	public void GetService_ReturnsUnderlyingNLEmbedding()
	{
		IEmbeddingGenerator<string, Embedding<float>> generator = new NLEmbeddingGenerator();
		var embedding = generator.GetService<NLEmbedding>();

		Assert.NotNull(embedding);
	}
}

[Category("NLEmbeddingGenerator")]
public class NLEmbeddingGeneratorInstantiationTests : EmbeddingGeneratorInstantiationTestsBase<NLEmbeddingGenerator>
{
	[Fact]
	public void LanguageConstructor_WithEnglish_CreatesInstance()
	{
		var generator = new NLEmbeddingGenerator(NLLanguage.English);
		Assert.NotNull(generator);
	}

	[Fact]
	public void LanguageConstructor_WithSpanish_CreatesInstance()
	{
		var generator = new NLEmbeddingGenerator(NLLanguage.Spanish);
		Assert.NotNull(generator);
	}

	[Fact]
	public void EmbeddingConstructor_WithValidEmbedding_CreatesInstance()
	{
		var embedding = NLEmbedding.GetSentenceEmbedding(NLLanguage.English);
		Assert.NotNull(embedding);

		var generator = new NLEmbeddingGenerator(embedding);
		Assert.NotNull(generator);
	}

	[Fact]
	public void EmbeddingConstructor_WithNull_ThrowsArgumentNullException()
	{
		Assert.Throws<ArgumentNullException>(() => new NLEmbeddingGenerator((NLEmbedding)null!));
	}
}

[Category("NLEmbeddingGenerator")]
public class NLEmbeddingGeneratorSimilarityTests : EmbeddingGeneratorSimilarityTestsBase<NLEmbeddingGenerator>
{
}

#endif
