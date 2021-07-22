using Microsoft.Extensions.DependencyInjection;
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
			builder.Services.AddSingleton<IEnergySaverListenerManager>(svcs => new EnergySaverListenerManager());
			builder.Services.AddTransient<ITicker>(svcs => new NativeTicker(svcs.GetRequiredService<IEnergySaverListenerManager>()));
#else
			builder.Services.AddTransient<ITicker>(svcs => new NativeTicker());
#endif
			builder.Services.AddTransient<IAnimationManager>(svcs => new AnimationManager(svcs.GetRequiredService<ITicker>()));

			return builder;
		}
	}
}