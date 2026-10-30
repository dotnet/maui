using System;
using Microsoft.AspNetCore.Components.WebView.Maui;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Maui.Controls.Hosting;
using Microsoft.Maui.Hosting;
using Microsoft.Maui.Platform;

namespace Microsoft.Maui.Gtk.UnitTests.Web;

/// <summary>
/// A MAUI app with the GTK core backend, the GTK Essentials (the app package file system HybridWebView reads) and the Blazor WebView
/// handler, for the WebKit tests. Touch it on the GTK thread.
/// </summary>
public static class WebTestApp
{
	static MauiApp? s_app;

	public static MauiApp App => s_app ??= Build();

	static MauiApp Build()
	{
		var builder = MauiApp.CreateBuilder();
		builder.UseMauiApp<TestApplication>()
			.UseMauiGtkCore()
			.UseMauiGtkEssentials(o => o.ApplicationId = "org.maui.gtk.webtests");
		builder.Services.AddMauiBlazorWebView().UseGtkPlatformHandler();
		return builder.Build();
	}

	/// <summary>Creates the handler of <paramref name="view"/> (connecting it and mapping every property).</summary>
	public static THandler CreateHandler<THandler>(IElement view)
		where THandler : class, IElementHandler =>
		(THandler)view.ToHandler(new MauiContext(App.Services));
}
