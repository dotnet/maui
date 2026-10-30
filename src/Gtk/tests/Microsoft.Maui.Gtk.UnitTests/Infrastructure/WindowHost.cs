using System;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Platform;

namespace Microsoft.Maui.Gtk.UnitTests
{
	/// <summary>
	/// Hosts MAUI content in a real <c>global::Gtk.Window</c> through the GTK window handler, so the full measure/arrange chain
	/// (root panel, page, layouts, handlers) runs.
	/// </summary>
	public sealed class WindowHost : IDisposable
	{
		public WindowHost(View content, int width = 800, int height = 600)
		{
			// Undecorated, so the content area is exactly the window size the tests allocate (no client-side title bar).
			PlatformWindow = new global::Gtk.Window { Decorated = false };
			Context = GtkTestApp.NewContext();
			Context.AddWeakSpecific<global::Gtk.Window>(PlatformWindow);

			Content = content;
			Page = new ContentPage { Content = content };
			Window = new Microsoft.Maui.Controls.Window(Page);

			PlatformWindow.SetWindowHandler(Window, Context);
			PlatformWindow.SetDefaultSize(width, height);
			PlatformWindow.Present();

			GtkTestHost.Pump(PlatformWindow, 8);
			GtkTestHost.Resize(PlatformWindow, width, height);
		}

		public global::Gtk.Window PlatformWindow { get; }

		public MauiContext Context { get; }

		public Microsoft.Maui.Controls.Window Window { get; }

		public ContentPage Page { get; }

		public View Content { get; }

		/// <summary>The native widget of a virtual view.</summary>
		public global::Gtk.Widget Widget(IView view) =>
			(global::Gtk.Widget)view.ToPlatform();

		/// <summary>The bounds of a virtual view's widget relative to the window content (the root panel), excluding the window decoration.</summary>
		public Gdk.Rectangle Bounds(IView view) =>
			GtkTestHost.BoundsIn(Widget(view), PlatformWindow.Child);

		public void Pump(int rounds = GtkTestHost.DefaultPumpRounds) => GtkTestHost.Pump(PlatformWindow, rounds);

		public void Dispose() => GtkTestHost.Retire(PlatformWindow);
	}
}
