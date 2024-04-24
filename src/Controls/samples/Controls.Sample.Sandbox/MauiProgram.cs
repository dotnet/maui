using Microsoft.Maui;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Controls.Hosting;
using Microsoft.Maui.Hosting;

namespace Maui.Controls.Sample
{
	public static class MauiProgram
	{
		public static MauiApp CreateMauiApp() =>
			MauiApp
				.CreateBuilder()
#if __ANDROID__ || __IOS__
				.UseMauiMaps()
#endif
				.UseMauiApp<App>()
				.Build();
	}

	class App : Application
	{
		protected override Window CreateWindow(IActivationState? activationState)
		{
			// To test shell scenarios, change this to true
			bool useShell = false;

			if (!useShell)
			{
				var wnd = new Window(new NavigationPage(new MainPage()));

				var titlebar = new TitlebarSample();
				titlebar.HeightRequest = 60;
				wnd.TitleBar = titlebar;
				return wnd;
			}
			else
			{
				return new Window(new SandboxShell());
			}
		}
	}
}