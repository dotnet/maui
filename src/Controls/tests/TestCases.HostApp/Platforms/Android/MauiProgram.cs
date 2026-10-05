using Android.Gms.Common;
using Android.Gms.Maps;
using Microsoft.Maui.LifecycleEvents;
using Microsoft.Maui.Maps.Handlers;

namespace Maui.Controls.Sample;

public static partial class MauiProgram
{
	static void ConfigureAndroidMaps(MauiAppBuilder builder)
	{
		// Loading Maps at startup lets Google Play module updates kill unrelated UI tests.
		builder.ConfigureMauiHandlers(handlers =>
		{
			handlers.AddMauiMaps();
			handlers.AddHandler<Microsoft.Maui.Controls.Maps.Map, UITestMapHandler>();
		})
		.ConfigureLifecycleEvents(events => events.AddAndroid(android =>
			android.OnCreate((activity, bundle) => MapHandler.Bundle = bundle)));
	}

	sealed class UITestMapHandler : MapHandler
	{
		protected override MapView CreatePlatformView()
		{
			var result = MapsInitializer.Initialize(Context, MapsInitializer.Renderer.Latest, null);
			if (result != ConnectionResult.Success)
				throw new InvalidOperationException($"Google Maps initialization failed with status {result}.");

			return base.CreatePlatformView();
		}
	}
}
