using System;
using System.Text.RegularExpressions;
using Microsoft.Maui.ApplicationModel;

namespace Microsoft.Maui.Essentials.Gtk;

/// <summary>Reads the desktop color scheme from the XDG settings portal (<c>org.freedesktop.appearance color-scheme</c>).</summary>
static class PortalSettings
{
	const int CallTimeoutMs = 750;

	/// <summary>
	/// Maps the portal value: 0 = no preference, 1 = prefer dark, 2 = prefer light.
	/// </summary>
	internal static AppTheme ToAppTheme(int? colorScheme) => colorScheme switch
	{
		1 => AppTheme.Dark,
		2 => AppTheme.Light,
		_ => AppTheme.Unspecified,
	};

	/// <summary>
	/// Extracts the unsigned integer from a printed GVariant such as <c>(&lt;&lt;uint32 1&gt;&gt;,)</c> or <c>(&lt;&lt;1&gt;&gt;,)</c>.
	/// </summary>
	internal static int? ParsePrintedUInt(string? printed)
	{
		if (string.IsNullOrEmpty(printed))
			return null;

		var m = Regex.Match(printed, @"(?<![\w.])(\d+)(?![\w.])");
		return m.Success && int.TryParse(m.Groups[1].Value, out var value) ? value : null;
	}

	/// <summary>Queries the portal; null when there is no session bus, no portal or the call fails.</summary>
	internal static int? TryReadColorScheme()
	{
		try
		{
			using var connection = GLib.Bus.GetSync(GLib.BusType.Session, null!);
			if (connection is null)
				return null;

			using var args = GLib.Variant.NewTuple(new[]
			{
				new GLib.Variant("org.freedesktop.appearance"),
				new GLib.Variant("color-scheme"),
			});

			using var reply = connection.CallSync(
				"org.freedesktop.portal.Desktop",
				"/org/freedesktop/portal/desktop",
				"org.freedesktop.portal.Settings",
				"Read",
				args,
				null!,
				GLib.DBusCallFlags.None,
				CallTimeoutMs,
				null!);

			return ParsePrintedUInt(reply?.Print(true));
		}
		catch (Exception)
		{
			return null;
		}
	}

	/// <summary>Fallback when no portal answers: GTK's own settings.</summary>
	internal static AppTheme ReadGtkSettingsTheme()
	{
		try
		{
			var settings = global::Gtk.Settings.Default;
			if (settings is null)
				return AppTheme.Unspecified;

			if (settings.ApplicationPreferDarkTheme)
				return AppTheme.Dark;

			var themeName = settings.GtkThemeName;
			if (themeName is not null && themeName.IndexOf("dark", StringComparison.OrdinalIgnoreCase) >= 0)
				return AppTheme.Dark;

			return AppTheme.Light;
		}
		catch (Exception)
		{
			return AppTheme.Unspecified;
		}
	}
}
