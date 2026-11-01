using System;
using System.Diagnostics;
using System.IO;
using System.Runtime.InteropServices;

namespace Microsoft.Maui.Gtk.Fonts
{
	/// <summary>
	/// Extracts the fonts registered with <c>ConfigureFonts</c> onto disk and makes them known to Pango, and so to every
	/// GTK widget in this process.
	/// </summary>
	/// <remarks>
	/// On Linux a font must be where fontconfig looks: the file is written to the user font directory
	/// (<c>$XDG_DATA_HOME/fonts/&lt;application&gt;</c>) so it survives; elsewhere (Windows, macOS, no home) to a per-application
	/// temp folder. It is then added to the running process: with <c>pango_font_map_add_font_file</c> on Pango 1.56 or newer
	/// (the route that works on every OS, including the win32 font map of the Windows GTK runtime), otherwise with
	/// <c>FcConfigAppFontAddFile</c> followed by a Pango cache flush, which only helps a fontconfig backed font map
	/// (<see cref="FontConfigHelper"/>). Fonts with the same file name are not extracted twice.
	/// </remarks>
	public sealed class GtkEmbeddedFontLoader : IEmbeddedFontLoader
	{
		/// <inheritdoc/>
		public string? LoadFont(EmbeddedFont font)
		{
			if (font is null || string.IsNullOrWhiteSpace(font.FontName))
				return null;

			// The registered name is a resource file name: never let it escape the cache directory.
			var fontName = Path.GetFileName(font.FontName);
			if (string.IsNullOrWhiteSpace(fontName))
				return null;

			string filePath;
			try
			{
				var directory = GetFontDirectory();
				Directory.CreateDirectory(directory);
				filePath = Path.Combine(directory, fontName);
			}
			catch (Exception ex)
			{
				Debug.WriteLine(ex);
				return null;
			}

			if (File.Exists(filePath))
			{
				Register(filePath);
				return filePath;
			}

			try
			{
				if (font.ResourceStream is null)
					return null;

				using (var fileStream = File.Create(filePath))
					font.ResourceStream.CopyTo(fileStream);

				Register(filePath);
				return filePath;
			}
			catch (Exception ex)
			{
				Debug.WriteLine(ex);

				try
				{
					File.Delete(filePath);
				}
				catch (Exception deleteException)
				{
					Debug.WriteLine(deleteException);
				}
			}

			return null;
		}

		/// <summary>
		/// Opens a font registered by file name (<c>AddFont("Name.ttf")</c>): the file MauiFont copies next to the application
		/// (<c>Resources/Fonts</c>, <c>Fonts</c> or the application folder). Installed into the stock <c>FontRegistrar</c> by
		/// <c>UseMauiGtkCore</c>. Only the file name is used, never a directory part of <paramref name="filename"/>.
		/// </summary>
		internal static Stream? OpenNativeFont(string filename)
		{
			var name = Path.GetFileName(filename);
			if (string.IsNullOrEmpty(name))
				return null;

			var baseDirectory = AppContext.BaseDirectory;
			string[] folders =
			{
				Path.Combine(baseDirectory, "Resources", "Fonts"),
				Path.Combine(baseDirectory, "Fonts"),
				baseDirectory,
			};

			foreach (var folder in folders)
			{
				var candidate = Path.Combine(folder, name);
				if (File.Exists(candidate))
					return File.OpenRead(candidate);
			}

			return null;
		}

		static void Register(string filePath)
		{
			if (!FontConfigHelper.TryAddFontFileWithPango(filePath))
				FontConfigHelper.AddAppFontFile(filePath);
		}

		static string GetFontDirectory()
		{
			var appName = System.Reflection.Assembly.GetEntryAssembly()?.GetName().Name ?? "Maui";
			var home = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile);

			if (!string.IsNullOrEmpty(home) && !RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
			{
				// freedesktop.org basedir spec: fontconfig scans $XDG_DATA_HOME/fonts recursively.
				var dataHome = Environment.GetEnvironmentVariable("XDG_DATA_HOME");
				if (string.IsNullOrWhiteSpace(dataHome))
					dataHome = Path.Combine(home, ".local", "share");

				return Path.Combine(dataHome, "fonts", appName);
			}

			// Windows (development hosts) and homeless environments: a per-application temp folder.
			return Path.Combine(Path.GetTempPath(), appName, "Fonts");
		}
	}
}
