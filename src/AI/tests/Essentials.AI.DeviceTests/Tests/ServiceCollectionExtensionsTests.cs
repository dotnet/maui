using Microsoft.Extensions.AI;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace Microsoft.Maui.Essentials.AI.DeviceTests;

public partial class ServiceCollectionExtensionsTests
{
	[Fact]
	public void AddPlatformChatClient_RegistersChatClient()
	{
		var services = new ServiceCollection();
		services.AddPlatformChatClient();

		var serviceProvider = services.BuildServiceProvider();
		var chatClient = serviceProvider.GetService<IChatClient>();

		Assert.NotNull(chatClient);
	}

	[Fact]
	public void AddPlatformChatClient_RegistersCorrectPlatformImplementation()
	{
		var services = new ServiceCollection();
		services.AddPlatformChatClient();

		var serviceProvider = services.BuildServiceProvider();
		var chatClient = serviceProvider.GetService<IChatClient>();

		Assert.NotNull(chatClient);

#if IOS || MACCATALYST
		Assert.IsType<AppleIntelligenceChatClient>(chatClient);
#elif ANDROID
		Assert.IsType<MLKitGenAIChatClient>(chatClient);
#elif WINDOWS
		Assert.IsType<WindowsAIChatClient>(chatClient);
#endif
	}

	[Fact]
	public void AddPlatformChatClient_WithSingletonLifetimeReturnsSameInstance()
	{
		var services = new ServiceCollection();
		services.AddPlatformChatClient(ServiceLifetime.Singleton);

		var serviceProvider = services.BuildServiceProvider();
		var chatClient1 = serviceProvider.GetService<IChatClient>();
		var chatClient2 = serviceProvider.GetService<IChatClient>();

		Assert.NotNull(chatClient1);
		Assert.NotNull(chatClient2);
		Assert.Same(chatClient1, chatClient2);
	}

	[Fact]
	public void AddPlatformChatClient_WithTransientLifetimeReturnsDifferentInstances()
	{
		var services = new ServiceCollection();
		services.AddPlatformChatClient(ServiceLifetime.Transient);

		var serviceProvider = services.BuildServiceProvider();
		var chatClient1 = serviceProvider.GetService<IChatClient>();
		var chatClient2 = serviceProvider.GetService<IChatClient>();

		Assert.NotNull(chatClient1);
		Assert.NotNull(chatClient2);
		Assert.NotSame(chatClient1, chatClient2);
	}

	[Fact]
	public void AddPlatformChatClient_WithScopedLifetimeReturnsSameInstanceInScope()
	{
		var services = new ServiceCollection();
		services.AddPlatformChatClient(ServiceLifetime.Scoped);

		var serviceProvider = services.BuildServiceProvider();

		using var scope1 = serviceProvider.CreateScope();
		var chatClient1a = scope1.ServiceProvider.GetService<IChatClient>();
		var chatClient1b = scope1.ServiceProvider.GetService<IChatClient>();

		using var scope2 = serviceProvider.CreateScope();
		var chatClient2 = scope2.ServiceProvider.GetService<IChatClient>();

		Assert.NotNull(chatClient1a);
		Assert.NotNull(chatClient1b);
		Assert.NotNull(chatClient2);
		Assert.Same(chatClient1a, chatClient1b);
		Assert.NotSame(chatClient1a, chatClient2);
	}

	[Fact]
	public void AddPlatformChatClient_ThrowsArgumentNullExceptionWhenServicesIsNull()
	{
		IServiceCollection? services = null;
		Assert.Throws<ArgumentNullException>(() => services!.AddPlatformChatClient());
	}
}
