using Microsoft.Extensions.AI;
using Xunit;

namespace Microsoft.Maui.Essentials.AI.DeviceTests;

/// <summary>
/// Base class for embedding generator GetService tests.
/// </summary>
/// <typeparam name="T">The embedding generator type to test.</typeparam>
public abstract class EmbeddingGeneratorGetServiceTestsBase<T>
	where T : class, IEmbeddingGenerator<string, Embedding<float>>, new()
{
	/// <summary>
	/// Gets the expected provider name for metadata.
	/// </summary>
	protected abstract string ExpectedProviderName { get; }

	/// <summary>
	/// Gets the expected default model ID for metadata.
	/// </summary>
	protected abstract string ExpectedDefaultModelId { get; }

	[Fact]
	public void GetService_ReturnsMetadataWithCorrectProviderAndModel()
	{
		IEmbeddingGenerator<string, Embedding<float>> generator = new T();
		var metadata = generator.GetService<EmbeddingGeneratorMetadata>();

		Assert.NotNull(metadata);
		Assert.Equal(ExpectedProviderName, metadata.ProviderName);
		Assert.Equal(ExpectedDefaultModelId, metadata.DefaultModelId);
	}

	[Fact]
	public void GetService_ReturnsItselfForMatchingType()
	{
		IEmbeddingGenerator<string, Embedding<float>> generator = new T();
		var self = generator.GetService<T>();

		Assert.NotNull(self);
		Assert.Same(generator, self);
	}

	[Fact]
	public void GetService_ReturnsNullForUnknownService()
	{
		IEmbeddingGenerator generator = new T();
		var unknownService = generator.GetService(typeof(string));

		Assert.Null(unknownService);
	}

	[Fact]
	public void GetService_ReturnsNullForKeyedServices()
	{
		IEmbeddingGenerator generator = new T();
		var keyedService = generator.GetService(typeof(EmbeddingGeneratorMetadata), "some-key");

		Assert.Null(keyedService);
	}

	[Fact]
	public void GetService_ThrowsForNullServiceType()
	{
		IEmbeddingGenerator generator = new T();

		Assert.Throws<ArgumentNullException>(() => generator.GetService(null!));
	}
}
