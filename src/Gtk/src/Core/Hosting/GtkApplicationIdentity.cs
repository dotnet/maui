using System;
using System.IO;
using System.Linq;
using System.Reflection;

namespace Microsoft.Maui.Gtk.Hosting
{
	/// <summary>
	/// The identity of a MAUI application on GTK 4: the <c>GApplication</c> id and the application icon. Both come from the
	/// SDK build: <c>ApplicationId</c> is emitted as <c>AssemblyMetadata</c> and the icon files as
	/// <c>Resources/Icons/hicolor/&lt;size&gt;x&lt;size&gt;/apps/&lt;name&gt;.png</c> (Resizetizer, <c>MauiIcon</c>).
	/// </summary>
	internal static class GtkApplicationIdentity
	{
		internal const string IconsFolder = "Icons";

		/// <summary>
		/// The id to start the <c>Gtk.Application</c> with: the <c>ApplicationId</c> assembly metadata when it is a valid
		/// <c>GApplication</c> id, else one derived from the assembly name. The generated <c>.desktop</c> file is named after
		/// the metadata id, which lets the desktop match the running window to its entry and icon.
		/// </summary>
		internal static string ResolveApplicationId(Assembly? assembly)
		{
			var metadata = assembly?.GetCustomAttributes<AssemblyMetadataAttribute>().FirstOrDefault(a => a.Key == "ApplicationId")?.Value;
			return IsValidApplicationId(metadata) ? metadata! : DeriveApplicationId(assembly);
		}

		/// <summary>The id <c>org.maui.&lt;assembly name&gt;</c> with every character that D-Bus names do not allow replaced.</summary>
		internal static string DeriveApplicationId(Assembly? assembly)
		{
			var name = assembly?.GetName().Name ?? "maui";
			var chars = new char[name.Length];
			for (int i = 0; i < name.Length; i++)
				chars[i] = char.IsLetterOrDigit(name[i]) && name[i] < 128 ? char.ToLowerInvariant(name[i]) : '_';

			var id = new string(chars);
			if (id.Length == 0 || char.IsDigit(id[0]))
				id = "app" + id;

			return "org.maui." + id;
		}

		/// <summary>The rules of <c>g_application_id_is_valid</c>: at most 255 characters, two or more dot separated elements of letters, digits, '_' or '-' that do not start with a digit.</summary>
		internal static bool IsValidApplicationId(string? id)
		{
			if (string.IsNullOrEmpty(id) || id!.Length > 255)
				return false;

			var elements = id.Split('.');
			if (elements.Length < 2)
				return false;

			foreach (var element in elements)
			{
				if (element.Length == 0 || char.IsDigit(element[0]))
					return false;

				foreach (var c in element)
				{
					var ok = (c < 128 && char.IsLetterOrDigit(c)) || c == '_' || c == '-';
					if (!ok)
						return false;
				}
			}

			return true;
		}

		/// <summary>The name (without extension) of the application icon below <c>Resources/Icons/hicolor</c> of <paramref name="baseDirectory"/>, preferring the largest size; <see langword="null"/> when there is none.</summary>
		internal static string? FindIconName(string baseDirectory)
		{
			var hicolor = Path.Combine(baseDirectory, "Resources", IconsFolder, "hicolor");
			if (!Directory.Exists(hicolor))
				return null;

			string? best = null;
			var bestSize = -1;
			foreach (var sizeDirectory in Directory.GetDirectories(hicolor))
			{
				var apps = Path.Combine(sizeDirectory, "apps");
				if (!Directory.Exists(apps))
					continue;

				var file = Directory.GetFiles(apps, "*.png").OrderBy(f => f, StringComparer.Ordinal).FirstOrDefault();
				if (file is null)
					continue;

				var size = ParseSize(Path.GetFileName(sizeDirectory));
				if (size > bestSize)
				{
					bestSize = size;
					best = Path.GetFileNameWithoutExtension(file);
				}
			}

			return best;
		}

		// "128x128" -> 128; anything else sorts last.
		static int ParseSize(string directoryName)
		{
			var x = directoryName.IndexOf('x', StringComparison.Ordinal);
			return x > 0 && int.TryParse(directoryName.AsSpan(0, x), out var size) ? size : 0;
		}

		/// <summary>
		/// Makes the application icon known to GTK: adds <c>Resources/Icons</c> to the icon theme search path and uses the icon
		/// as the default window icon. Does nothing without icon files.
		/// </summary>
		internal static void InstallIcon(string baseDirectory)
		{
			var name = FindIconName(baseDirectory);
			if (name is null)
				return;

			var display = Gdk.Display.Default;
			if (display is null)
				return;

			global::Gtk.IconTheme.GetForDisplay(display).AddSearchPath(Path.Combine(baseDirectory, "Resources", IconsFolder));
			global::Gtk.Window.DefaultIconName = name;
		}
	}
}
