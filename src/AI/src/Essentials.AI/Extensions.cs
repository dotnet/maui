using Microsoft.Extensions.AI;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace Microsoft.Maui.Essentials.AI;

public static class Extensions
{

#if IOS || MACCATALYST || MACOS
	public static IHostApplicationBuilder AddAppleIntelligenceChatClient(this IHostApplicationBuilder builder, ServiceLifetime lifetime = ServiceLifetime.Singleton)
	{
		_ = builder ?? throw new ArgumentNullException(nameof(builder));

		builder.Services.Add(new ServiceDescriptor(typeof(AppleIntelligenceChatClient), _ => new AppleIntelligenceChatClient(), lifetime));
		builder.Services.Add(new ServiceDescriptor(typeof(IChatClient), provider => provider.GetRequiredService<AppleIntelligenceChatClient>(), lifetime));
		
		return builder;
	}
#endif

#if ANDROID
	public static IHostApplicationBuilder AddMLKitGenAIChatClient(this IHostApplicationBuilder builder, ServiceLifetime lifetime = ServiceLifetime.Singleton)
	{
		_ = builder ?? throw new ArgumentNullException(nameof(builder));

		builder.Services.Add(new ServiceDescriptor(typeof(MLKitGenAIChatClient), _ => new MLKitGenAIChatClient(), lifetime));
		builder.Services.Add(new ServiceDescriptor(typeof(IChatClient), provider => provider.GetRequiredService<MLKitGenAIChatClient>(), lifetime));
		
		return builder;
	}
#endif

#if WINDOWS
	public static IHostApplicationBuilder AddWindowsAIChatClient(this IHostApplicationBuilder builder, ServiceLifetime lifetime = ServiceLifetime.Singleton)
	{
		_ = builder ?? throw new ArgumentNullException(nameof(builder));

		builder.Services.Add(new ServiceDescriptor(typeof(WindowsAIChatClient), _ => new WindowsAIChatClient(), lifetime));
		builder.Services.Add(new ServiceDescriptor(typeof(IChatClient), provider => provider.GetRequiredService<WindowsAIChatClient>(), lifetime));
		
		return builder;
	}
#endif

	public static IHostApplicationBuilder AddPlatformChatClient(this IHostApplicationBuilder builder, ServiceLifetime lifetime = ServiceLifetime.Singleton)
	{
		_ = builder ?? throw new ArgumentNullException(nameof(builder));

#if IOS || MACCATALYST || MACOS
		builder.AddAppleIntelligenceChatClient(lifetime);
#elif ANDROID
		builder.AddMLKitGenAIChatClient(lifetime);
#elif WINDOWS
		builder.AddWindowsAIChatClient(lifetime);
#endif

		return builder;
	}
}
