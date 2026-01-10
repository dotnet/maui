#if IOS || MACCATALYST
using Microsoft.Extensions.AI;
using NaturalLanguage;
using Xunit;

namespace Microsoft.Maui.Essentials.AI.DeviceTests;

public class NLContextualEmbeddingGeneratorGetServiceTests
{
	[Fact]
	public void GetService_ReturnsMetadataWithCorrectProviderAndModel()
	{
		IEmbeddingGenerator<string, Embedding<float>> generator = new NaturalLanguageContextualEmbeddingGenerator();
		var metadata = generator.GetService<EmbeddingGeneratorMetadata>();

		Assert.NotNull(metadata);
		Assert.Equal("apple", metadata.ProviderName);
		Assert.Equal("natural-language-contextual", metadata.DefaultModelId);
	}

	[Fact]
	public void GetService_ReturnsUnderlyingNLContextualEmbedding()
	{
		IEmbeddingGenerator<string, Embedding<float>> generator = new NaturalLanguageContextualEmbeddingGenerator();
		var embedding = generator.GetService<NLContextualEmbedding>();

		Assert.NotNull(embedding);
	}

	[Fact]
	public void GetService_ReturnsItselfForMatchingType()
	{
		IEmbeddingGenerator<string, Embedding<float>> generator = new NaturalLanguageContextualEmbeddingGenerator();
		var self = generator.GetService<NaturalLanguageContextualEmbeddingGenerator>();

		Assert.NotNull(self);
		Assert.Same(generator, self);
	}

	[Fact]
	public void GetService_ReturnsNullForUnknownService()
	{
		IEmbeddingGenerator generator = new NaturalLanguageContextualEmbeddingGenerator();
		var unknownService = generator.GetService(typeof(string));

		Assert.Null(unknownService);
	}

	[Fact]
	public void GetService_ReturnsNullForKeyedServices()
	{
		IEmbeddingGenerator generator = new NaturalLanguageContextualEmbeddingGenerator();
		var keyedService = generator.GetService(typeof(EmbeddingGeneratorMetadata), "some-key");

		Assert.Null(keyedService);
	}

	[Fact]
	public void GetService_ThrowsForNullServiceType()
	{
		IEmbeddingGenerator generator = new NaturalLanguageContextualEmbeddingGenerator();

		Assert.Throws<ArgumentNullException>(() => generator.GetService(null!));
	}
}
#endif
