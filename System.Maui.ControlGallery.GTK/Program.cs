using GLib;
using System.Maui;
using System.Maui.ControlGallery.GTK;
using System;
using System.Maui.Platform.GTK;
using System.Maui.Platform.GTK.Renderers;
using System.Maui.Controls;
using System.Maui.Maps.GTK;

[assembly: ExportRenderer(typeof(DisposePage), typeof(DisposePageRenderer))]
[assembly: ExportRenderer(typeof(DisposeLabel), typeof(DisposeLabelRenderer))]
namespace System.Maui.ControlGallery.GTK
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
            System.Maui.Maui.SetFlags("CarouselView_Experimental");
            FormsMaps.Init(string.Empty);
            System.Maui.Maui.Init();
            var app = new App();
            var window = new FormsWindow();
            window.LoadApplication(app);
            window.SetApplicationTitle("System.Maui GTK# Backend");
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