using Microsoft.Extensions.DependencyInjection;
using Microsoft.Maui.Controls.Compatibility;
using Microsoft.Maui.Hosting;
using Microsoft.Maui.LifecycleEvents;
using TDeviceInfo = Tizen.UIExtensions.Common.DeviceInfo;

namespace Microsoft.Maui.Controls.Compatibility.Hosting
{
	public static partial class AppHostBuilderExtensions
	{
		internal static MauiAppBuilder ConfigureCompatibilityLifecycleEvents(this MauiAppBuilder builder) =>
			   builder.ConfigureLifecycleEvents(events => events.AddTizen(OnConfigureLifeCycle));

		static void OnConfigureLifeCycle(ITizenLifecycleBuilder tizen)
		{
			tizen.OnPreCreate((app) =>
			{
				// This is the initial Init to set up any system services registered by
				// Forms.Init(). This happens before any UI has appeared.
				// This creates a dummy MauiContext.

				var services = MauiApplication.Current.Services;
				MauiContext mauiContext = new MauiContext(services);
				ActivationState state = new ActivationState(mauiContext);

#pragma warning disable CS0612 // Type or member is obsolete
				var options = services.GetService<InitializationOptions>();
				if (options == null)
				{
					options = new InitializationOptions()
					{
						DisplayResolutionUnit = TDeviceInfo.DisplayResolutionUnit.ToCompatibility(TDeviceInfo.ViewPortWidth)
					};
				}
				else
				{
					options.Context = options.Context ?? MauiApplication.Current;
					TDeviceInfo.DisplayResolutionUnit = options.DisplayResolutionUnit.ToDeviceInfo();
				}
				options.Flags |= InitializationFlags.SkipRenderers;
#pragma warning disable CS0612 // Type or member is obsolete
				Forms.Init(state, options);
#pragma warning disable CS0612 // Type or member is obsolete
			})
			.OnMauiContextCreated((mauiContext) =>
			{
				// This is the final Init that sets up the real context from the application.

				var state = new ActivationState(mauiContext!);
#pragma warning disable CS0612 // Type or member is obsolete
				Forms.Init(state);
#pragma warning disable CS0612 // Type or member is obsolete
			});
		}
	}
}
