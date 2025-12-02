using Microsoft.Extensions.AI;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace Microsoft.Maui.Essentials.AI.DeviceTests;

public partial class ServiceCollectionExtensionsTests
{
#if ANDROID
	[Fact]
	public void AddMLKitGenAIChatClient_RegistersChatClient()
	{
		var services = new ServiceCollection();
		services.AddMLKitGenAIChatClient();

		var serviceProvider = services.BuildServiceProvider();
		var chatClient = serviceProvider.GetService<IChatClient>();

		Assert.NotNull(chatClient);
		Assert.IsType<MLKitGenAIChatClient>(chatClient);
	}

	[Fact]
	public void AddMLKitGenAIChatClient_RegistersConcreteType()
	{
		var services = new ServiceCollection();
		services.AddMLKitGenAIChatClient();

		var serviceProvider = services.BuildServiceProvider();
		var concreteClient = serviceProvider.GetService<MLKitGenAIChatClient>();

		Assert.NotNull(concreteClient);
	}

	[Fact]
	public void AddMLKitGenAIChatClient_ThrowsArgumentNullExceptionWhenServicesIsNull()
	{
		IServiceCollection? services = null;
		Assert.Throws<ArgumentNullException>(() => services!.AddMLKitGenAIChatClient());
	}
#endif
}
