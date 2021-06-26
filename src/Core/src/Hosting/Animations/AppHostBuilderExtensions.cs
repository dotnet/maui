using Microsoft.Extensions.DependencyInjection;
using Microsoft.Maui.Animations;
#if __ANDROID__
using Microsoft.Maui.Platform;
#endif

namespace Microsoft.Maui.Hosting
{
	public static partial class AppHostBuilderExtensions
	{
		public static IAppHostBuilder ConfigureAnimations(this IAppHostBuilder builder)
		{
			builder.ConfigureServices(services =>
			{
#if __ANDROID__
				services.AddSingleton<IEnergySaverListenerManager>(svcs => new EnergySaverListenerManager());
				services.AddSingleton<ITicker>(svcs => new NativeTicker(svcs.GetRequiredService<IEnergySaverListenerManager>()));
#else
				services.AddSingleton<ITicker>(svcs => new NativeTicker());
#endif
				services.AddSingleton<IAnimationManager>(svcs => new AnimationManager(svcs.GetRequiredService<ITicker>()));
			});

			return builder;
		}
	}
}