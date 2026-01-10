#if IOS || MACCATALYST
using Microsoft.Extensions.AI;
using Xunit;

using PlatformChatClient = Microsoft.Maui.Essentials.AI.AppleIntelligenceChatClient;

namespace Microsoft.Maui.Essentials.AI.DeviceTests;

public class AppleIntelligenceChatClientResponseTests
{
	[Fact]
	public async Task GetResponseAsync_ReturnsNonNullResponse()
	{
		var client = new PlatformChatClient();
		var messages = new List<ChatMessage>
		{
			new(ChatRole.User, "Hello")
		};

		var response = await client.GetResponseAsync(messages);

		Assert.NotNull(response);
	}
}
#endif
