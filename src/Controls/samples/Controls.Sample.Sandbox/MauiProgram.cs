using Microsoft.Maui;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Controls.Hosting;
using Microsoft.Maui.Hosting;
using Microsoft.Maui.Graphics;

namespace Maui.Controls.Sample
{
	public static class MauiProgram
	{
		public static MauiApp CreateMauiApp() =>
			MauiApp
				.CreateBuilder()
				.UseMauiMaps()
				.UseMauiApp<App>()
				.Build();
	}

	class App : Application
	{
		protected override Window CreateWindow(IActivationState? activationState)
		{
			// To test shell scenarios, change this to true
			bool useShell = true;

			if (!useShell)
			{
				var detail = new NavigationPage(new MainPage()){
					BarBackgroundColor = Colors.Transparent,
					BarTextColor = Colors.White
				};

				Microsoft.Maui.Controls.PlatformConfiguration.iOSSpecific.NavigationPage.SetIsNavigationBarTranslucent(detail, true);

				return new Window(detail);
			}
			else
			{
				return new Window(new SandboxShell());
			}
		}
	}
}