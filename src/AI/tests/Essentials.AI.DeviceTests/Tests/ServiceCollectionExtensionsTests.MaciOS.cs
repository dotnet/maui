using Microsoft.Extensions.AI;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace Microsoft.Maui.Essentials.AI.DeviceTests;

public partial class ServiceCollectionExtensionsTests
{
#if IOS || MACCATALYST
	[Fact]
	public void AddAppleIntelligenceChatClient_RegistersChatClient()
	{
		var services = new ServiceCollection();
		services.AddAppleIntelligenceChatClient();

		var serviceProvider = services.BuildServiceProvider();
		var chatClient = serviceProvider.GetService<IChatClient>();

		Assert.NotNull(chatClient);
		Assert.IsType<AppleIntelligenceChatClient>(chatClient);
	}

	[Fact]
	public void AddAppleIntelligenceChatClient_RegistersConcreteType()
	{
		var services = new ServiceCollection();
		services.AddAppleIntelligenceChatClient();

		var serviceProvider = services.BuildServiceProvider();
		var concreteClient = serviceProvider.GetService<AppleIntelligenceChatClient>();

		Assert.NotNull(concreteClient);
	}

	[Fact]
	public void AddAppleIntelligenceChatClient_ThrowsArgumentNullExceptionWhenServicesIsNull()
	{
		IServiceCollection? services = null;
		Assert.Throws<ArgumentNullException>(() => services!.AddAppleIntelligenceChatClient());
	}
#endif
}
