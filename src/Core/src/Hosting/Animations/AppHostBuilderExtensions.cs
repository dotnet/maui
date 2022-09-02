using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Maui.Animations;

#if __ANDROID__
using Microsoft.Maui.Platform;
#endif

namespace Microsoft.Maui.Hosting
{
	public static partial class AppHostBuilderExtensions
	{
		public static MauiAppBuilder ConfigureAnimations(this MauiAppBuilder builder)
		{
#if __ANDROID__
			builder.Services.TryAddSingleton<IEnergySaverListenerManager>(svcs => new EnergySaverListenerManager());
			builder.Services.TryAddScoped<ITicker>(svcs => new PlatformTicker(svcs.GetRequiredService<IEnergySaverListenerManager>()));
#else
			builder.Services.TryAddScoped<ITicker>(svcs => new PlatformTicker());
#endif
			builder.Services.TryAddScoped<IAnimationManager>(svcs => new AnimationManager(svcs.GetRequiredService<ITicker>()));

			return builder;
		}
	}
}