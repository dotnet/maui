using System;
using Microsoft.AspNetCore.Components.WebView.Maui;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Maui.Controls.Hosting;
using Microsoft.Maui.Hosting;
using Microsoft.Maui.Gtk.Web;
using Microsoft.Maui.Platform;
using Xunit;

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

	/// <summary>
	/// Puts the platform view of <paramref name="handler"/> into a window and pumps until its engine is ready, when the engine needs a window
	/// (the WebView2 host creates its controller on realize); a WebKit view or a placeholder needs none. Returns the window to retire, or <see langword="null"/>.
	/// </summary>
	public static global::Gtk.Window? Present(IElementHandler handler)
	{
		if (handler.PlatformView is not MauiGtkWebView2Host host)
			return null;

		var window = new global::Gtk.Window { Decorated = false, DefaultWidth = 800, DefaultHeight = 600, Child = host };
		window.Present();
		GtkTestHost.Resize(window, 800, 600);
		Assert.True(GtkTestHost.PumpUntil(() => host.Engine.Core is not null, window, 20000), "the WebView2 controller was not created");
		return window;
	}

	/// <summary>Creates the handler of <paramref name="view"/> (connecting it and mapping every property).</summary>
	public static THandler CreateHandler<THandler>(IElement view)
		where THandler : class, IElementHandler =>
		(THandler)view.ToHandler(new MauiContext(App.Services));
}
