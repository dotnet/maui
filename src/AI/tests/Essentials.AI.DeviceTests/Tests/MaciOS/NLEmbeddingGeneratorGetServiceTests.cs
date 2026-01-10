#if IOS || MACCATALYST
using Microsoft.Extensions.AI;
using NaturalLanguage;
using Xunit;

namespace Microsoft.Maui.Essentials.AI.DeviceTests;

public class NLEmbeddingGeneratorGetServiceTests
{
	[Fact]
	public void GetService_ReturnsMetadataWithCorrectProviderAndModel()
	{
		IEmbeddingGenerator<string, Embedding<float>> generator = new NaturalLanguageEmbeddingGenerator();
		var metadata = generator.GetService<EmbeddingGeneratorMetadata>();

		Assert.NotNull(metadata);
		Assert.Equal("apple", metadata.ProviderName);
		Assert.Equal("natural-language", metadata.DefaultModelId);
	}

	[Fact]
	public void GetService_ReturnsUnderlyingNLEmbedding()
	{
		IEmbeddingGenerator<string, Embedding<float>> generator = new NaturalLanguageEmbeddingGenerator();
		var embedding = generator.GetService<NLEmbedding>();

		Assert.NotNull(embedding);
	}

	[Fact]
	public void GetService_ReturnsItselfForMatchingType()
	{
		IEmbeddingGenerator<string, Embedding<float>> generator = new NaturalLanguageEmbeddingGenerator();
		var self = generator.GetService<NaturalLanguageEmbeddingGenerator>();

		Assert.NotNull(self);
		Assert.Same(generator, self);
	}

	[Fact]
	public void GetService_ReturnsNullForUnknownService()
	{
		IEmbeddingGenerator generator = new NaturalLanguageEmbeddingGenerator();
		var unknownService = generator.GetService(typeof(string));

		Assert.Null(unknownService);
	}

	[Fact]
	public void GetService_ReturnsNullForKeyedServices()
	{
		IEmbeddingGenerator generator = new NaturalLanguageEmbeddingGenerator();
		var keyedService = generator.GetService(typeof(EmbeddingGeneratorMetadata), "some-key");

		Assert.Null(keyedService);
	}

	[Fact]
	public void GetService_ThrowsForNullServiceType()
	{
		IEmbeddingGenerator generator = new NaturalLanguageEmbeddingGenerator();

		Assert.Throws<ArgumentNullException>(() => generator.GetService(null!));
	}
}
#endif
