using System;
using System.Linq;
using Microsoft.Maui;
using Microsoft.Maui.Hosting;

namespace Maui.Gtk.Sample;

/// <summary>The Linux/GTK 4 entry point.</summary>
public static class Program
{
	/// <summary>
	/// <c>--smoke</c> opens the window, lets it lay out, checks the result and exits (used for headless runs under xvfb).
	/// </summary>
	[STAThread] // GDK on Windows initialises COM (OleInitialize), which needs a single-threaded apartment.
	public static int Main(string[] args)
	{
		var smoke = args.Contains("--smoke", StringComparer.Ordinal);
		SmokeTest.Enabled = smoke;

		int exitCode = new LinuxApp().Run(args.Where(a => a != "--smoke").ToArray());

		if (smoke)
		{
			Console.WriteLine(SmokeTest.Succeeded ? "SMOKE OK" : "SMOKE FAILED: " + SmokeTest.Failure);
			return SmokeTest.Succeeded ? exitCode : 1;
		}

		return exitCode;
	}
}

sealed class LinuxApp : MauiGtkApplication
{
	protected override MauiApp CreateMauiApp() => MauiProgram.CreateMauiApp();
}
