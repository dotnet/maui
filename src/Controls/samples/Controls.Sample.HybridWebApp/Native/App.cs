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
			Title = ".NET MAUI Playground — packaged",
			Width = 1180,
			Height = 820
		};
#if DEBUG
		if (MauiProgram.Development?.Upstream is not null)
			window.Title = ".NET MAUI Playground — Vite";
#endif
		window.Destroying += (_, _) => page.Stop();
		return window;
	}
}
