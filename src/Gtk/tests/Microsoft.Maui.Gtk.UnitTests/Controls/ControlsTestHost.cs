using System;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Controls.Hosting;
using Microsoft.Maui.Hosting;
using Microsoft.Maui.Platform;

namespace Microsoft.Maui.Gtk.UnitTests
{
	/// <summary>An application whose first window shows an empty page; tests replace the page.</summary>
	public class ControlsTestApplication : Microsoft.Maui.Controls.Application
	{
		protected override Microsoft.Maui.Controls.Window CreateWindow(IActivationState? activationState) =>
			new(new ContentPage());
	}

	/// <summary>Builds (once) a real <see cref="MauiApp"/> with <c>UseMauiGtk()</c> and creates handlers and windows from it.</summary>
	public static class ControlsTestApp
	{
		static MauiApp? s_app;

		/// <summary>The shared app; must be touched on the GTK thread.</summary>
		public static MauiApp App => s_app ??= MauiApp.CreateBuilder()
			.UseMauiApp<ControlsTestApplication>()
			.UseMauiGtk()
			.Build();

		/// <summary>A fresh context over the shared app's services.</summary>
		public static MauiContext NewContext() => new(App.Services);
	}

	/// <summary>
	/// Hosts a Controls <see cref="Page"/> as the page of a <see cref="Microsoft.Maui.Controls.Window"/> on a real
	/// <c>Gtk.Window</c>, through the GTK window handler, so the whole chain (toolbar, navigation, tabs, flyout, layout) runs.
	/// </summary>
	public sealed class ControlsWindowHost : IDisposable
	{
		public ControlsWindowHost(Page page, int width = 800, int height = 600)
		{
			PlatformWindow = new global::Gtk.Window();
			Context = ControlsTestApp.NewContext();
			Context.AddWeakSpecific(PlatformWindow);

			Page = page;
			Window = new Microsoft.Maui.Controls.Window(page);

			// The application owns its windows (pages only appear once the window has a parent); attach the window like CreateWindow does.
			((Microsoft.Maui.Controls.Application)ControlsTestApp.App.Services.GetRequiredService<IApplication>()).AddWindow(Window);
			PlatformWindow.SetWindowHandler(Window, Context);
			PlatformWindow.SetDefaultSize(width, height);
			PlatformWindow.Present();

			GtkTestHost.Pump(PlatformWindow, 8);

			// A bare X server has no window manager and never activates a toplevel; pages appear when the window activates.
			((Microsoft.Maui.Gtk.Handlers.GtkWindowHandler)Window.Handler!).UpdateActive(true);
			GtkTestHost.Pump(PlatformWindow, 8);
			GtkTestHost.Resize(PlatformWindow, width, height);
		}

		public global::Gtk.Window PlatformWindow { get; }

		public MauiContext Context { get; }

		public Microsoft.Maui.Controls.Window Window { get; }

		public Page Page { get; }

		/// <summary>The native widget of a virtual view.</summary>
		public global::Gtk.Widget Widget(IView view) => (global::Gtk.Widget)view.ToPlatform();

		/// <summary>The bounds of a virtual view's widget relative to the window content.</summary>
		public Gdk.Rectangle Bounds(IView view) => GtkTestHost.BoundsIn(Widget(view), PlatformWindow.Child);

		public void Pump(int rounds = GtkTestHost.DefaultPumpRounds) => GtkTestHost.Pump(PlatformWindow, rounds);

		public void Dispose() => GtkTestHost.Retire(PlatformWindow);
	}
}
