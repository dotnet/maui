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
			builder.Services.TryAddTransient<ITicker>(svcs => new NativeTicker(svcs.GetRequiredService<IEnergySaverListenerManager>()));
#else
			builder.Services.TryAddTransient<ITicker>(svcs => new NativeTicker());
#endif
			builder.Services.TryAddTransient<IAnimationManager>(svcs => new AnimationManager(svcs.GetRequiredService<ITicker>()));

			return builder;
		}
	}
}