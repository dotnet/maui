using Microsoft.Extensions.AI;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace Microsoft.Maui.Essentials.AI.DeviceTests;

public partial class ServiceCollectionExtensionsTests
{
#if WINDOWS
	[Fact]
	public void AddWindowsAIChatClient_RegistersChatClient()
	{
		var services = new ServiceCollection();
		services.AddWindowsAIChatClient();

		var serviceProvider = services.BuildServiceProvider();
		var chatClient = serviceProvider.GetService<IChatClient>();

		Assert.NotNull(chatClient);
		Assert.IsType<WindowsAIChatClient>(chatClient);
	}

	[Fact]
	public void AddWindowsAIChatClient_RegistersConcreteType()
	{
		var services = new ServiceCollection();
		services.AddWindowsAIChatClient();

		var serviceProvider = services.BuildServiceProvider();
		var concreteClient = serviceProvider.GetService<WindowsAIChatClient>();

		Assert.NotNull(concreteClient);
	}

	[Fact]
	public void AddWindowsAIChatClient_ThrowsArgumentNullExceptionWhenServicesIsNull()
	{
		IServiceCollection? services = null;
		Assert.Throws<ArgumentNullException>(() => services!.AddWindowsAIChatClient());
	}
#endif
}
