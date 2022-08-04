using Microsoft.Maui;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Controls.Hosting;
using Microsoft.Maui.Hosting;
using Microsoft.Maui.LifecycleEvents;
using System;

#if ANDROID
using Android.Gms.Common;
using Android.Gms.Maps;
#endif

namespace Maui.Controls.Sample
{
	public static class MauiProgram
	{
		public static MauiApp CreateMauiApp() =>
			MauiApp
				.CreateBuilder()
				.ConfigureMauiHandlers(handlers =>
				{
#if __ANDROID__ || __IOS__
					handlers.AddMauiMapsControlsHandlers();
#endif
				})
				.ConfigureLifecycleEvents(events =>
				{
#if __ANDROID__
					// Log everything in this one
					events.AddAndroid(android => android
						.OnCreate((a, b) =>
						{

							Microsoft.Maui.Maps.Handlers.MapHandler.Bundle = b;
#pragma warning disable CS0618 // Type or member is obsolete
							if (GooglePlayServicesUtil.IsGooglePlayServicesAvailable(a) == ConnectionResult.Success)
#pragma warning restore 618
							{
								try
								{
									MapsInitializer.Initialize(a);
								}
								catch (Exception e)
								{
									Console.WriteLine("Google Play Services Not Found");
									Console.WriteLine("Exception: {0}", e);
								}
							}
#pragma warning restore CS0618 // Type or member is obsolete
						}));
#endif
				})
			
				.UseMauiApp<App>()
				.Build();
	}

	class App : Application
	{
		protected override Window CreateWindow(IActivationState activationState) =>
			new Window(new MapPage());
	}
}