using Microsoft.Extensions.AI;
using Xunit;

namespace Microsoft.Maui.Essentials.AI.DeviceTests;

/// <summary>
/// Base class for ChatClient instantiation tests.
/// Provides common tests for any IChatClient implementation.
/// </summary>
/// <typeparam name="T">The concrete ChatClient type to test.</typeparam>
public abstract class ChatClientInstantiationTestsBase<T>
	where T : class, IChatClient, IDisposable, new()
{
	[Fact]
	public void ChatClient_CanBeCreated()
	{
		var client = new T();
		Assert.NotNull(client);
	}

	[Fact]
	public void ChatClient_CanBeDisposed()
	{
		var client = new T();

		// Should not throw
		client.Dispose();
	}

	[Fact]
	public void ChatClient_CanBeDisposedMultipleTimes()
	{
		var client = new T();

		// Should not throw
		client.Dispose();
		client.Dispose();
	}

	[Fact]
	public void ChatClient_MultipleInstancesCanBeCreated()
	{
		var client1 = new T();
		var client2 = new T();

		Assert.NotNull(client1);
		Assert.NotNull(client2);
		Assert.NotSame(client1, client2);
	}
}
