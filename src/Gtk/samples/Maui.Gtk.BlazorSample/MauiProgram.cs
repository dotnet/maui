using Microsoft.AspNetCore.Components.WebView.Maui;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Maui.Controls.Hosting;
using Microsoft.Maui.Hosting;

namespace Maui.Gtk.BlazorSample;

public static class MauiProgram
{
	public static MauiApp CreateMauiApp()
	{
		var builder = MauiApp.CreateBuilder();
		builder.UseMauiApp<App>().UseMauiGtkCore();

		// Registers the BlazorWebView services, then the GTK (WebKitGTK 6) handler. Needs libwebkitgtk-6.0 at run time.
		builder.Services.AddMauiBlazorWebView().UseGtkPlatformHandler();

		return builder.Build();
	}
}
