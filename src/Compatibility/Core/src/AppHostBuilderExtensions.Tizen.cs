using Microsoft.Maui.Hosting;
using Microsoft.Maui.LifecycleEvents;
using Microsoft.Maui.Controls.Compatibility;
using Microsoft.Extensions.DependencyInjection;

namespace Microsoft.Maui.Controls.Hosting
{
	public static partial class AppHostBuilderExtensions
	{
		internal static IAppHostBuilder ConfigureCompatibilityLifecycleEvents(this IAppHostBuilder builder) =>
			   builder.ConfigureLifecycleEvents(events => events.AddTizen(OnConfigureLifeCycle));

		static void OnConfigureLifeCycle(ITizenLifecycleBuilder tizen)
		{
			tizen.OnPreCreate((app) =>
			{
				// This is the initial Init to set up any system services registered by
				// Forms.Init(). This happens before any UI has appeared.
				// This creates a dummy MauiContext.

				var services = MauiApplication.Current.Services;
				MauiContext mauiContext = new MauiContext(services, CoreUIAppContext.GetInstance(MauiApplication.Current));
				ActivationState state = new ActivationState(mauiContext);

				var options = services.GetService<InitializationOptions>();
				if (options == null)
				{
					options = new InitializationOptions(MauiApplication.Current)
					{
						DisplayResolutionUnit = DisplayResolutionUnit.DP()
					};
				}
				else
				{
					options.Context = options.Context ?? MauiApplication.Current;
				}
				options.Flags |= InitializationFlags.SkipRenderers;
				Forms.Init(state, options);
			})
			.OnMauiContextCreated((mauiContext) =>
			{
				// This is the final Init that sets up the real context from the application.

				var state = new ActivationState(mauiContext!);
				Forms.Init(state);
			});
		}
	}
}
