using Microsoft.Maui;
using Microsoft.Maui.Controls.Hosting;
using Microsoft.Maui.Controls.Xaml;
using Microsoft.Maui.Hosting;

#pragma warning disable CS0618 // XamlCompilationAttribute is deprecated, remove this in .NET 12
[assembly: XamlCompilation(XamlCompilationOptions.Compile)]
#pragma warning restore CS0618

namespace Maui.Controls.Sample.Profiling
{
	public static class MauiProgram
	{
		public static MauiApp CreateMauiApp()
		{
			var builder = MauiApp.CreateBuilder();

			builder
				.UseMauiApp<App>()
				.ConfigureFonts(fonts =>
				{
					fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
				});

			return builder.Build();
		}
	}
}
