using Microsoft.Extensions.AI;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace Microsoft.Maui.Essentials.AI;

public static class Extensions
{

#if IOS || MACCATALYST || MACOS
	public static IServiceCollection AddAppleIntelligenceChatClient(this IServiceCollection services, ServiceLifetime lifetime = ServiceLifetime.Singleton)
	{
		_ = services ?? throw new ArgumentNullException(nameof(services));

		services.Add(new ServiceDescriptor(typeof(AppleIntelligenceChatClient), typeof(AppleIntelligenceChatClient), lifetime));
		services.Add(new ServiceDescriptor(typeof(IChatClient), provider => provider.GetRequiredService<AppleIntelligenceChatClient>(), lifetime));
		
		return services;
	}
#endif

	public static IServiceCollection AddPlatformChatClient(this IServiceCollection services, ServiceLifetime lifetime = ServiceLifetime.Singleton)
	{
		_ = services ?? throw new ArgumentNullException(nameof(services));

#if IOS || MACCATALYST || MACOS
		services.AddAppleIntelligenceChatClient(lifetime);
#endif

		return services;
	}
}
