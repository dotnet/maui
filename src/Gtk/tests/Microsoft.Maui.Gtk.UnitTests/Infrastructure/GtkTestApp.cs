using System;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Controls.Hosting;
using Microsoft.Maui.Controls.Xaml;
using Microsoft.Maui.Hosting;
using Microsoft.Maui.Platform;

namespace Microsoft.Maui.Gtk.UnitTests
{
	/// <summary>A minimal Controls application used to build a MAUI app wired to the GTK backend.</summary>
	public class TestApplication : Microsoft.Maui.Controls.Application
	{
		protected override Microsoft.Maui.Controls.Window CreateWindow(IActivationState? activationState) =>
			new(new ContentPage());
	}

	/// <summary>Builds (once) a real <see cref="MauiApp"/> with Controls and the GTK core backend, and creates handlers from it.</summary>
	public static class GtkTestApp
	{
		static MauiApp? s_app;

		/// <summary>The shared app; must be touched on the GTK thread.</summary>
		public static MauiApp App => s_app ??= MauiApp.CreateBuilder()
			.UseMauiApp<TestApplication>()
			.UseMauiGtkCore()
			.Build();

		/// <summary>A fresh context over the shared app's services.</summary>
		public static MauiContext NewContext() => new(App.Services);

		/// <summary>Creates the handler of <paramref name="view"/> (connecting it and mapping every property).</summary>
		public static THandler CreateHandler<THandler>(IElement view)
			where THandler : class, IElementHandler
		{
			var handler = view.ToHandler(NewContext());
			return (THandler)handler;
		}

		/// <summary>Creates the handler of <paramref name="view"/> and returns its platform view as <typeparamref name="TWidget"/>.</summary>
		public static TWidget CreateWidget<TWidget>(IView view)
			where TWidget : class
		{
			var handler = view.ToHandler(NewContext());
			return (TWidget)handler.PlatformView!;
		}
	}
}
