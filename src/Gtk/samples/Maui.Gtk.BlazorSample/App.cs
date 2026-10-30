using Microsoft.Maui;
using Microsoft.Maui.Controls;

namespace Maui.Gtk.BlazorSample;

public sealed class App : Application
{
	protected override Window CreateWindow(IActivationState? activationState) =>
		new(new MainPage()) { Title = "Blazor on GTK 4", Width = 640, Height = 480 };
}
