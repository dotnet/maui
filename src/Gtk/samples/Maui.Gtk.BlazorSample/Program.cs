using System;
using Microsoft.Maui;
using Microsoft.Maui.Hosting;

namespace Maui.Gtk.BlazorSample;

/// <summary>The Linux/GTK 4 entry point.</summary>
public static class Program
{
	[STAThread] // GDK on Windows initialises COM (OleInitialize), which needs a single-threaded apartment.
	public static int Main(string[] args) => new LinuxApp().Run(args);
}

sealed class LinuxApp : MauiGtkApplication
{
	protected override MauiApp CreateMauiApp() => MauiProgram.CreateMauiApp();
}
