using Microsoft.Maui;
using Microsoft.Maui.Controls;

namespace Maui.Gtk.Sample;

public sealed class App : Application
{
	protected override Window CreateWindow(IActivationState? activationState)
	{
		var page = new MainPage();
		SmokeTest.Attach(page);
		return new Window(page) { Title = "MAUI on GTK 4", Width = 480, Height = 640 };
	}
}
