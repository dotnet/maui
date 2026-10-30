using Microsoft.Maui.Controls.Hosting;
using Microsoft.Maui.Hosting;

namespace Maui.Gtk.Sample;

public static class MauiProgram
{
	public static MauiApp CreateMauiApp() =>
		MauiApp.CreateBuilder()
			.UseMauiApp<App>()
			// Must follow UseMauiApp (and precede any AppendToMapping customisation). The Controls backend
			// replaces this call with UseMauiGtk(), which also registers Controls and Essentials.
			.UseMauiGtkCore()
			.Build();
}
