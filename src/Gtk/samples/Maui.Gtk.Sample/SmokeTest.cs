using System;
using Microsoft.Maui;
using Microsoft.Maui.Controls;

namespace Maui.Gtk.Sample;

/// <summary>
/// Headless self-check behind <c>--smoke</c>: after the window has been shown and laid out, verifies that the page was
/// measured and arranged, then quits the application.
/// </summary>
static class SmokeTest
{
	public static bool Enabled { get; set; }

	public static bool Succeeded { get; private set; }

	public static string Failure { get; private set; } = "the window was never shown";

	public static void Attach(MainPage page)
	{
		if (!Enabled)
			return;

		GLib.Timeout.Add(1500, () =>
		{
			try
			{
				Verify(page);
			}
			catch (Exception ex)
			{
				Failure = ex.Message;
			}

			((GLib.Application)MauiGtkApplication.Current!.GtkApplication).Quit();
			return false;
		});
	}

	static void Verify(MainPage page)
	{
		if (page.TitleLabel.Frame.Width <= 0 || page.TitleLabel.Frame.Height <= 0)
		{
			Failure = $"the title label was not arranged ({page.TitleLabel.Frame})";
			return;
		}

		if (page.TitleLabel.Handler?.PlatformView is not global::Gtk.Label)
		{
			Failure = "the title label has no GTK widget";
			return;
		}

		Succeeded = true;
	}
}
