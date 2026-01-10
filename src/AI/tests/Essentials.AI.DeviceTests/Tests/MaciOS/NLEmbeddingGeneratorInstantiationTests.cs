#if IOS || MACCATALYST
using NaturalLanguage;
using Xunit;

namespace Microsoft.Maui.Essentials.AI.DeviceTests;

public class NLEmbeddingGeneratorInstantiationTests
{
	[Fact]
	public void DefaultConstructor_CreatesInstance()
	{
		var generator = new NaturalLanguageEmbeddingGenerator();
		Assert.NotNull(generator);
	}

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

	[Fact]
	public void MultipleInstances_CanBeCreated()
	{
		var generator1 = new NaturalLanguageEmbeddingGenerator();
		var generator2 = new NaturalLanguageEmbeddingGenerator();

		Assert.NotNull(generator1);
		Assert.NotNull(generator2);
		Assert.NotSame(generator1, generator2);
	}
}
#endif
