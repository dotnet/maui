using System;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Maui;
using Microsoft.Maui.Controls.Compatibility;
using Microsoft.Maui.Controls.Compatibility.Hosting;
using Microsoft.Maui.Controls.Hosting;
using Microsoft.Maui.Devices;
using Microsoft.Maui.Hosting;

namespace Microsoft.Maui.Controls.Compatibility.ControlGallery
{
	public static class MauiProgram
	{
		internal static bool UseBlazor = false;

		public static MauiApp CreateMauiApp()
		{
			var builder = CreateMauiAppBuilder();
			return builder.Build();
		}

		public static MauiAppBuilder CreateMauiAppBuilder()
		{
			var builder = MauiApp.CreateBuilder();
			builder
				.UseMauiApp<App>()
				.UseMauiCompatibility()
				.ConfigureMauiHandlers(handlers =>
				{
					handlers.AddCompatibilityRenderers(AppDomain.CurrentDomain.GetAssemblies());
				})
				.ConfigureImageSources(sources =>
				{
					sources.AddCompatibilityServices(AppDomain.CurrentDomain.GetAssemblies());
				})
				.ConfigureFonts(fonts =>
				{
					fonts.AddCompatibilityFonts(new FontRegistrar(new EmbeddedFontLoader()), AppDomain.CurrentDomain.GetAssemblies());
				})
				.ConfigureEffects(effects =>
				{
					effects.AddCompatibilityEffects(AppDomain.CurrentDomain.GetAssemblies());
				});

			if (DeviceInfo.Platform == DevicePlatform.Tizen)
			{
				//Some controls still need to use legacy renderers on Tizen.
				builder.UseMauiCompatibility();
			}

			DependencyService.Register(AppDomain.CurrentDomain.GetAssemblies());

			return builder;
		}
	}
}
