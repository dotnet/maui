using Microsoft.Extensions.AI;
using Xunit;

namespace Microsoft.Maui.Essentials.AI.DeviceTests;

/// <summary>
/// Base class for ChatClient GetService tests.
/// Provides common tests for any IChatClient implementation.
/// </summary>
/// <typeparam name="T">The concrete ChatClient type to test.</typeparam>
public abstract class ChatClientGetServiceTestsBase<T>
	where T : class, IChatClient, new()
{
	/// <summary>
	/// Gets the expected provider name returned by ChatClientMetadata.
	/// </summary>
	protected abstract string ExpectedProviderName { get; }

	/// <summary>
	/// Gets the expected default model ID returned by ChatClientMetadata.
	/// </summary>
	protected abstract string ExpectedDefaultModelId { get; }

	[Fact]
	public void GetService_ReturnsMetadataWithCorrectProviderAndModel()
	{
		var client = new T();
		var metadata = client.GetService<ChatClientMetadata>();

		Assert.NotNull(metadata);

		Assert.Equal(ExpectedProviderName, metadata.ProviderName);
		Assert.Equal(ExpectedDefaultModelId, metadata.DefaultModelId);
	}

	[Fact]
	public void GetService_ReturnsNullForUnknownService()
	{
		IChatClient client = new T();
		var unknownService = client.GetService(typeof(string));
		Assert.Null(unknownService);
	}

	[Fact]
	public void GetService_ReturnsNullForKeyedServices()
	{
		IChatClient client = new T();
		var keyedService = client.GetService(typeof(ChatClientMetadata), "some-key");
		Assert.Null(keyedService);
	}

	[Fact]
	public void GetService_ReturnsItselfForMatchingType()
	{
		IChatClient client = new T();
		var self = client.GetService(typeof(T));

		Assert.NotNull(self);
		Assert.Same(client, self);
	}

	[Fact]
	public void GetService_ReturnsItselfForIChatClientType()
	{
		IChatClient client = new T();
		var self = client.GetService(typeof(IChatClient));
		Assert.NotNull(self);
		Assert.Same(client, self);
	}
}
