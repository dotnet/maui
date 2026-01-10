#if IOS || MACCATALYST
using NaturalLanguage;
using Xunit;

namespace Microsoft.Maui.Essentials.AI.DeviceTests;

public class NLContextualEmbeddingGeneratorInstantiationTests
{
	[Fact]
	public void DefaultConstructor_CreatesInstance()
	{
		var generator = new NaturalLanguageContextualEmbeddingGenerator();
		Assert.NotNull(generator);
	}

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

	[Fact]
	public void MultipleInstances_CanBeCreated()
	{
		var generator1 = new NaturalLanguageContextualEmbeddingGenerator();
		var generator2 = new NaturalLanguageContextualEmbeddingGenerator();

		Assert.NotNull(generator1);
		Assert.NotNull(generator2);
		Assert.NotSame(generator1, generator2);
	}
}
#endif
