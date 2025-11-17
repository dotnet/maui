using Microsoft.Extensions.AI;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace Microsoft.Maui.Essentials.AI;

public static class Extensions
{

#if IOS || MACCATALYST || MACOS
	public static IServiceCollection AddAppleIntelligenceChatClient(this IServiceCollection services, ServiceLifetime lifetime = ServiceLifetime.Singleton)
	{
		_ = services ?? throw new ArgumentNullException(nameof(services));

		services.Add(new ServiceDescriptor(typeof(AppleIntelligenceChatClient), _ => new AppleIntelligenceChatClient(), lifetime));
		services.Add(new ServiceDescriptor(typeof(IChatClient), provider => provider.GetRequiredService<AppleIntelligenceChatClient>(), lifetime));
		
		return services;
	}
#endif

#if ANDROID
	public static IServiceCollection AddMLKitGenAIChatClient(this IServiceCollection services, ServiceLifetime lifetime = ServiceLifetime.Singleton)
	{
		_ = services ?? throw new ArgumentNullException(nameof(services));

		services.Add(new ServiceDescriptor(typeof(MLKitGenAIChatClient), _ => new MLKitGenAIChatClient(), lifetime));
		services.Add(new ServiceDescriptor(typeof(IChatClient), provider => provider.GetRequiredService<MLKitGenAIChatClient>(), lifetime));
		
		return services;
	}
#endif

#if WINDOWS
	public static IServiceCollection AddWindowsAIChatClient(this IServiceCollection services, ServiceLifetime lifetime = ServiceLifetime.Singleton)
	{
		_ = services ?? throw new ArgumentNullException(nameof(services));

		services.Add(new ServiceDescriptor(typeof(WindowsAIChatClient), _ => new WindowsAIChatClient(), lifetime));
		services.Add(new ServiceDescriptor(typeof(IChatClient), provider => provider.GetRequiredService<WindowsAIChatClient>(), lifetime));
		
		return services;
	}
#endif

	public static IServiceCollection AddPlatformChatClient(this IServiceCollection services, ServiceLifetime lifetime = ServiceLifetime.Singleton)
	{
		_ = services ?? throw new ArgumentNullException(nameof(services));

#if IOS || MACCATALYST || MACOS
		services.AddAppleIntelligenceChatClient(lifetime);
#elif ANDROID
		services.AddMLKitGenAIChatClient(lifetime);
#elif WINDOWS
		services.AddWindowsAIChatClient(lifetime);
#endif

		return services;
	}
}
