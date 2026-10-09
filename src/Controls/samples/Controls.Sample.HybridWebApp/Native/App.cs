using Microsoft.Maui;
using Microsoft.Maui.Controls;

namespace Maui.Controls.Sample.HybridWebApp;

public sealed class App : Application
{
	protected override Window CreateWindow(IActivationState? activationState)
	{
		return new Window(new MainPage())
		{
			Title = "Hybrid web app — packaged content",
			Width = 1000,
			Height = 750
		};
	}
}
