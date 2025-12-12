using Xunit;

#if IOS || MACCATALYST
using PlatformChatClient = Microsoft.Maui.Essentials.AI.AppleIntelligenceChatClient;
#elif ANDROID
using PlatformChatClient = Microsoft.Maui.Essentials.AI.MLKitGenAIChatClient;
#elif WINDOWS
using PlatformChatClient = Microsoft.Maui.Essentials.AI.WindowsAIChatClient;
#endif

namespace Microsoft.Maui.Essentials.AI.DeviceTests;

public partial class ChatClientInstantiationTests
{
	[Fact]
	public void ChatClient_CanBeCreated()
	{
		var client = new PlatformChatClient();
		Assert.NotNull(client);
	}

	[Fact]
	public void ChatClient_CanBeDisposed()
	{
		var client = new PlatformChatClient();

		// Should not throw
		((IDisposable)client).Dispose();
	}

	[Fact]
	public void ChatClient_CanBeDisposedMultipleTimes()
	{
		var client = new PlatformChatClient();

		// Should not throw
		((IDisposable)client).Dispose();
		((IDisposable)client).Dispose();
	}

	[Fact]
	public void ChatClient_MultipleInstancesCanBeCreated()
	{
		var client1 = new PlatformChatClient();
		var client2 = new PlatformChatClient();

		Assert.NotNull(client1);
		Assert.NotNull(client2);
		Assert.NotSame(client1, client2);
	}
}
