using System;
using GLib;
using Xamarin.Forms;
using Xamarin.Forms.ControlGallery.GTK;
using Xamarin.Forms.Controls;
using Xamarin.Forms.Maps.GTK;
using Xamarin.Forms.Platform.GTK;
using Xamarin.Forms.Platform.GTK.Renderers;

[assembly: ExportRenderer(typeof(DisposePage), typeof(DisposePageRenderer))]
[assembly: ExportRenderer(typeof(DisposeLabel), typeof(DisposeLabelRenderer))]
namespace Xamarin.Forms.ControlGallery.GTK
{
	class Program
	{
		[STAThread]
		static void Main(string[] args)
		{
			ExceptionManager.UnhandledException += OnUnhandledException;

			GtkOpenGL.Init();
			GtkThemes.Init();
			Gtk.Application.Init();
			FormsMaps.Init(string.Empty);
			Forms.Init();
			var app = new App();
			var window = new FormsWindow();
			window.LoadApplication(app);
			window.SetApplicationTitle("Xamarin.Forms GTK# Backend");
			window.SetApplicationIcon("xamarinlogo.png");
			window.Show();
			Gtk.Application.Run();
		}

		private static void OnUnhandledException(UnhandledExceptionArgs args)
		{
			System.Diagnostics.Debug.WriteLine($"Unhandled GTK# exception: {args.ExceptionObject}");
		}
	}

	public class DisposePageRenderer : PageRenderer
	{
		protected override void Dispose(bool disposing)
		{
			if (disposing)
			{
				((DisposePage)Element).SendRendererDisposed();
			}

			base.Dispose(disposing);
		}
	}

	public class DisposeLabelRenderer : LabelRenderer
	{
		protected override void Dispose(bool disposing)
		{
			if (disposing)
			{
				((DisposeLabel)Element).SendRendererDisposed();
			}

			base.Dispose(disposing);
		}
	}
}