using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;

namespace Microsoft.Maui.Gtk.Fonts
{
	/// <summary>The native libraries <see cref="FontConfigHelper"/> talks to.</summary>
	internal enum FontLibrary
	{
		FontConfig,
		Pango,
		PangoCairo,
		PangoFt2,
		GObject,
	}

	/// <summary>
	/// Resolves the font related native libraries by their per-OS file names (a Linux soname, a Windows
	/// <c>*-1.0-0.dll</c> from the GTK runtime, a macOS dylib) with <see cref="NativeLibrary.TryLoad(string, out IntPtr)"/>,
	/// the same pattern as the other optional native dependencies of the backend. Nothing is registered globally
	/// (no <c>DllImportResolver</c>): callers get the export address and call it through a function pointer.
	/// </summary>
	internal static class FontNativeLibraries
	{
		static readonly object s_sync = new object();
		static readonly Dictionary<FontLibrary, IntPtr> s_loaded = new Dictionary<FontLibrary, IntPtr>();

		/// <summary>
		/// The file names to try for <paramref name="library"/> on <paramref name="platform"/>, best first. Sonames, not link
		/// names: <c>libfontconfig.so</c> only exists when the -dev package is installed.
		/// </summary>
		internal static IReadOnlyList<string> GetCandidates(FontLibrary library, OSPlatform platform)
		{
			string stem, soname, windows, dylib;

			switch (library)
			{
				case FontLibrary.FontConfig:
					stem = "fontconfig"; soname = "libfontconfig.so.1"; windows = "fontconfig-1.dll"; dylib = "libfontconfig.1.dylib";
					break;
				case FontLibrary.Pango:
					stem = "pango-1.0"; soname = "libpango-1.0.so.0"; windows = "pango-1.0-0.dll"; dylib = "libpango-1.0.0.dylib";
					break;
				case FontLibrary.PangoCairo:
					stem = "pangocairo-1.0"; soname = "libpangocairo-1.0.so.0"; windows = "pangocairo-1.0-0.dll"; dylib = "libpangocairo-1.0.0.dylib";
					break;
				case FontLibrary.PangoFt2:
					stem = "pangoft2-1.0"; soname = "libpangoft2-1.0.so.0"; windows = "pangoft2-1.0-0.dll"; dylib = "libpangoft2-1.0.0.dylib";
					break;
				default:
					stem = "gobject-2.0"; soname = "libgobject-2.0.so.0"; windows = "gobject-2.0-0.dll"; dylib = "libgobject-2.0.0.dylib";
					break;
			}

			if (platform == OSPlatform.Windows)
				return new[] { windows, stem + ".dll" };

			if (platform == OSPlatform.OSX)
				return new[] { dylib, "lib" + stem + ".dylib", "/opt/homebrew/lib/" + dylib, "/usr/local/lib/" + dylib };

			return new[] { soname, "lib" + stem + ".so" };
		}

		static OSPlatform CurrentPlatform =>
			RuntimeInformation.IsOSPlatform(OSPlatform.Windows) ? OSPlatform.Windows :
			RuntimeInformation.IsOSPlatform(OSPlatform.OSX) ? OSPlatform.OSX : OSPlatform.Linux;

		/// <summary>Loads (once) the library; <see cref="IntPtr.Zero"/> when no candidate name can be loaded.</summary>
		internal static IntPtr Load(FontLibrary library)
		{
			lock (s_sync)
			{
				if (s_loaded.TryGetValue(library, out var handle))
					return handle;

				handle = IntPtr.Zero;
				foreach (var name in GetCandidates(library, CurrentPlatform))
				{
					// Kept loaded for the life of the process, like the GTK libraries themselves.
					if (NativeLibrary.TryLoad(name, out handle))
						break;

					handle = IntPtr.Zero;
				}

				s_loaded[library] = handle;
				return handle;
			}
		}

		/// <summary>
		/// The address of <paramref name="symbol"/>. Throws <see cref="DllNotFoundException"/> or
		/// <see cref="EntryPointNotFoundException"/> like a failing P/Invoke would, so callers keep one "native part missing" path.
		/// </summary>
		internal static IntPtr GetExport(FontLibrary library, string symbol)
		{
			var handle = Load(library);
			if (handle == IntPtr.Zero)
				throw new DllNotFoundException($"{library}: none of {string.Join(", ", GetCandidates(library, CurrentPlatform))} could be loaded.");

			if (!NativeLibrary.TryGetExport(handle, symbol, out var address))
				throw new EntryPointNotFoundException($"{symbol} was not found in the {library} library.");

			return address;
		}
	}
}
