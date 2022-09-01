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
				.UseMauiMaps()
				.UseMauiApp<App>()
				.Build();
	}

	class App : Application
	{
		protected override Window CreateWindow(IActivationState activationState) =>
			new Window(new SandboxShell());
	}
}