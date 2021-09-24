using Microsoft.Extensions.DependencyInjection;
using Microsoft.Maui;
using Microsoft.Maui.Controls.Compatibility;
using Microsoft.Maui.Controls.Hosting;
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
				.ConfigureMauiHandlers(handlers =>
				{
					handlers.AddCompatibilityRenderers(Device.GetAssemblies());
				})
				.ConfigureImageSources(sources =>
				{
					sources.AddCompatibilityServices(Device.GetAssemblies());
				})
				.ConfigureFonts(fonts =>
				{
					fonts.AddCompatibilityFonts(Device.GetAssemblies());
				})
				.ConfigureEffects(effects =>
				{
					effects.AddCompatibilityEffects(Device.GetAssemblies());
				});

			DependencyService.Register(Device.GetAssemblies());

			return builder;
		}
	}
}
