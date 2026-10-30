using System;
using System.Reflection;
using Microsoft.Maui.Hosting;

namespace Microsoft.Maui.Essentials.Gtk;

/// <summary>Process-wide state shared by the GTK Essentials implementations.</summary>
static class GtkEssentialsContext
{
	internal static GtkEssentialsOptions Options { get; set; } = new GtkEssentialsOptions();

	/// <summary>The parent window for dialogs, or null when none is available. Call on the GTK thread.</summary>
	internal static global::Gtk.Window? GetParentWindow()
	{
		try
		{
			if (Options.ParentWindowProvider is { } provider)
				return provider();

			if (GLib.Application.Default is global::Gtk.Application app)
			{
				if (app.ActiveWindow is { } active)
					return active;

				var windows = app.Windows;
				if (windows is { Length: > 0 })
					return windows[0];
			}

			var toplevels = global::Gtk.Window.ListToplevels();
			return toplevels is { Length: > 0 } ? toplevels[0] : null;
		}
		catch (Exception)
		{
			return null;
		}
	}

	/// <summary>True when the loaded GTK is at least the given version.</summary>
	internal static bool IsGtkAtLeast(uint major, uint minor, uint micro = 0)
	{
		try
		{
			return global::Gtk.Global.CheckVersion(major, minor, micro) is null;
		}
		catch (Exception)
		{
			return false;
		}
	}

	internal static Assembly? EntryAssembly => Assembly.GetEntryAssembly();
}
