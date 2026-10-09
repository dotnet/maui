using Microsoft.Maui;
using Microsoft.Maui.Controls;

namespace Maui.Controls.Sample.HybridWebApp;

public sealed class App : Application
{
	protected override Window CreateWindow(IActivationState? activationState)
	{
		var page = new MainPage();
		var window = new Window(page)
		{
			Title = "Hybrid web app — packaged content",
			Width = 1000,
			Height = 750
		};
#if DEBUG
		if (MauiProgram.Development?.Upstream is not null)
			window.Title = "Hybrid web app — Vite development";
		window.Destroying += (_, _) => page.StopDevelopment();
#endif
		return window;
	}
}
