using Microsoft.Maui.Controls.Compatibility;
using Microsoft.Maui.Hosting;
using Microsoft.Maui.LifecycleEvents;

namespace Microsoft.Maui.Controls.Compatibility.Hosting
{
	public static partial class AppHostBuilderExtensions
	{
		internal static MauiAppBuilder ConfigureCompatibilityLifecycleEvents(this MauiAppBuilder builder) =>
			   builder.ConfigureLifecycleEvents(events => events.AddiOS(OnConfigureLifeCycle));

		static void OnConfigureLifeCycle(IiOSLifecycleBuilder iOS)
		{
			iOS.WillFinishLaunching((app, options) =>
				{
					// This is the initial Init to set up any system services registered by
					// Forms.Init(). This happens before any UI has appeared.
					// This creates a dummy MauiContext.

					var services = IPlatformApplication.Current.Services;
					var mauiContext = new MauiContext(services);
					var state = new ActivationState(mauiContext);
#pragma warning disable CS0612 // Type or member is obsolete
					Forms.Init(state, new InitializationOptions { Flags = InitializationFlags.SkipRenderers });
#pragma warning restore CS0612 // Type or member is obsolete
					return true;
				})
				.OnMauiContextCreated((mauiContext) =>
				{
					// This is the final Init that sets up the real context from the application.

					var state = new ActivationState(mauiContext);
#pragma warning disable CS0612 // Type or member is obsolete
					Forms.Init(state);
#pragma warning restore CS0612 // Type or member is obsolete
				});
		}
	}
}
