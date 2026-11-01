#nullable disable
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Text;

namespace Microsoft.Maui.Gtk.Fonts
{
	/// <summary>
	/// A thin layer over the fontconfig and Pango native libraries, used to make a font file that was written at
	/// runtime (an <see cref="EmbeddedFont"/> extracted by <see cref="GtkEmbeddedFontLoader"/>)
	/// usable by Pango <em>in this process</em>.
	/// <para>
	/// Dropping a file into <c>~/.local/share/fonts</c> is only honoured by processes that start
	/// afterwards - fontconfig builds its font set once, at first use. <c>FcConfigAppFontAddFile</c>
	/// injects the file into the current configuration immediately; the Pango cache flush that
	/// follows makes the new family visible to font maps that have already been created.
	/// </para>
	/// <para>
	/// The libraries are resolved per OS (<see cref="FontNativeLibraries"/>: Linux sonames, the Windows GTK runtime
	/// <c>*-1.0-0.dll</c>, macOS dylibs) and called through function pointers. On Windows Pango's default font map is the
	/// win32 one, not a fontconfig one: the font is added with <c>pango_font_map_add_font_file</c> and fontconfig only
	/// reads the family name out of the file. Everything here is best-effort: if a library cannot be loaded the helper
	/// turns itself off and callers fall back to name-based resolution.
	/// </para>
	/// </summary>
	internal static class FontConfigHelper
	{
		const int FcResultMatch = 0;
		const int FcSetApplication = 1;

		const string FcFamily = "family";
		const string FcFile = "file";

		static readonly object _sync = new object();
		static readonly HashSet<string> _addedFiles = new HashSet<string>(StringComparer.Ordinal);
		static readonly Dictionary<string, string> _familyNames = new Dictionary<string, string>(StringComparer.Ordinal);

		static bool _fontConfigUnavailable;
		static bool _pangoFlushUnavailable;

		static unsafe int FcConfigAppFontAddFile(IntPtr config, byte[] file)
		{
			var fn = (delegate* unmanaged[Cdecl]<IntPtr, byte*, int>)FontNativeLibraries.GetExport(FontLibrary.FontConfig, "FcConfigAppFontAddFile");
			fixed (byte* path = file)
				return fn(config, path);
		}

		static unsafe IntPtr FcFreeTypeQuery(byte[] file, uint id, IntPtr blanks, out int count)
		{
			var fn = (delegate* unmanaged[Cdecl]<byte*, uint, IntPtr, int*, IntPtr>)FontNativeLibraries.GetExport(FontLibrary.FontConfig, "FcFreeTypeQuery");
			int n;
			IntPtr pattern;
			fixed (byte* path = file)
				pattern = fn(path, id, blanks, &n);
			count = n;
			return pattern;
		}

		static unsafe int FcPatternGetString(IntPtr pattern, byte[] obj, int n, out IntPtr value)
		{
			var fn = (delegate* unmanaged[Cdecl]<IntPtr, byte*, int, IntPtr*, int>)FontNativeLibraries.GetExport(FontLibrary.FontConfig, "FcPatternGetString");
			IntPtr v;
			int result;
			fixed (byte* name = obj)
				result = fn(pattern, name, n, &v);
			value = v;
			return result;
		}

		static unsafe void FcPatternDestroy(IntPtr pattern)
			=> ((delegate* unmanaged[Cdecl]<IntPtr, void>)FontNativeLibraries.GetExport(FontLibrary.FontConfig, "FcPatternDestroy"))(pattern);

		static unsafe IntPtr FcConfigGetFonts(IntPtr config, int set)
			=> ((delegate* unmanaged[Cdecl]<IntPtr, int, IntPtr>)FontNativeLibraries.GetExport(FontLibrary.FontConfig, "FcConfigGetFonts"))(config, set);

		static unsafe IntPtr pango_cairo_font_map_get_default()
			=> ((delegate* unmanaged[Cdecl]<IntPtr>)FontNativeLibraries.GetExport(FontLibrary.PangoCairo, "pango_cairo_font_map_get_default"))();

		static unsafe void pango_fc_font_map_config_changed(IntPtr fontMap)
			=> ((delegate* unmanaged[Cdecl]<IntPtr, void>)FontNativeLibraries.GetExport(FontLibrary.PangoFt2, "pango_fc_font_map_config_changed"))(fontMap);

		// calling the fontconfig entry points on any other font map (win32, DirectWrite, CoreText) is a GLib critical
		static unsafe bool IsFcFontMap(IntPtr fontMap)
		{
			var getType = (delegate* unmanaged[Cdecl]<UIntPtr>)FontNativeLibraries.GetExport(FontLibrary.PangoFt2, "pango_fc_font_map_get_type");
			var isA = (delegate* unmanaged[Cdecl]<IntPtr, UIntPtr, int>)FontNativeLibraries.GetExport(FontLibrary.GObject, "g_type_check_instance_is_a");
			return isA(fontMap, getType()) != 0;
		}

		// Pango >= 1.56: loads a font file straight into a font map, without touching the fontconfig configuration.
		static unsafe int pango_font_map_add_font_file(IntPtr fontMap, byte[] filename, out IntPtr error)
		{
			var fn = (delegate* unmanaged[Cdecl]<IntPtr, byte*, IntPtr*, int>)FontNativeLibraries.GetExport(FontLibrary.Pango, "pango_font_map_add_font_file");
			IntPtr e = IntPtr.Zero;
			int result;
			fixed (byte* path = filename)
				result = fn(fontMap, path, &e);
			error = e;
			return result;
		}

		/// <summary>
		/// Logs the message of a <c>GError*</c> returned by GLib/Pango and frees it with <c>g_error_free</c>.
		/// <c>struct GError { GQuark domain; gint code; gchar *message; }</c>: the message pointer follows two 32-bit ints,
		/// at offset 8 on both 32- and 64-bit.
		/// </summary>
		static unsafe void ConsumeGError(IntPtr error, string filePath)
		{
			try
			{
				Debug.WriteLine($"pango_font_map_add_font_file refused '{filePath}': {FromUtf8(Marshal.ReadIntPtr(error, 8))}");
			}
			finally
			{
				((delegate* unmanaged[Cdecl]<IntPtr, void>)FontNativeLibraries.GetExport(FontLibrary.GLib, "g_error_free"))(error);
			}
		}

		/// <summary>
		/// Whether the fontconfig entry points are usable: false once a fontconfig call found the library (or
		/// a symbol) missing, e.g. a stripped container - in that case the font file is still written to the user
		/// font directory, it is just not visible until the next process start.
		/// </summary>
		internal static bool IsAvailable
		{
			get
			{
				lock (_sync)
					return !_fontConfigUnavailable;
			}
		}

		/// <summary>
		/// Adds <paramref name="filePath"/> to Pango's default font map with <c>pango_font_map_add_font_file</c>
		/// (Pango 1.56 or newer). Returns <see langword="false"/> when the entry point is missing, so the caller can fall
		/// back to <see cref="AddAppFontFile"/>.
		/// </summary>
		internal static bool TryAddFontFileWithPango(string filePath)
		{
			if (string.IsNullOrEmpty(filePath))
				return false;

			try
			{
				var fontMap = pango_cairo_font_map_get_default();
				if (fontMap == IntPtr.Zero)
					return false;

				var added = pango_font_map_add_font_file(fontMap, ToUtf8(filePath), out var error) != 0;
				if (error != IntPtr.Zero)
					ConsumeGError(error, filePath);

				return added;
			}
			catch (Exception ex) when (IsMissingNativeSymbol(ex))
			{
				return false;
			}
		}

		/// <summary>
		/// Registers <paramref name="filePath"/> with the process-wide fontconfig configuration and
		/// invalidates Pango's font caches. Idempotent - adding the same file twice is a no-op.
		/// </summary>
		/// <returns>true when fontconfig accepted the file.</returns>
		internal static bool AddAppFontFile(string filePath)
		{
			if (string.IsNullOrEmpty(filePath))
				return false;

			lock (_sync)
			{
				if (_fontConfigUnavailable)
					return false;

				if (_addedFiles.Contains(filePath))
					return true;

				try
				{
					if (FcConfigAppFontAddFile(IntPtr.Zero, ToUtf8(filePath)) == 0)
					{
						Debug.WriteLine($"FcConfigAppFontAddFile refused '{filePath}'.");
						return false;
					}

					_addedFiles.Add(filePath);
				}
				catch (Exception ex) when (IsMissingNativeSymbol(ex))
				{
					Debug.WriteLine($"libfontconfig unavailable, embedded fonts will not be visible until restart: {ex.Message}");
					_fontConfigUnavailable = true;
					return false;
				}
			}

			FlushPangoCaches();
			return true;
		}

		/// <summary>
		/// Reads the real family name inside a font file - the name Pango matches on, which is generally not the
		/// file name. Asks fontconfig (also on Windows, where it only parses the file) and falls back to reading the
		/// TrueType/OpenType <c>name</c> table directly (<see cref="FontFileNames"/>). Returns null when it cannot be determined.
		/// </summary>
		internal static string QueryFamilyName(string filePath)
		{
			if (string.IsNullOrEmpty(filePath))
				return null;

			lock (_sync)
			{
				if (_familyNames.TryGetValue(filePath, out var cached))
					return cached;

				string family = null;

				if (!_fontConfigUnavailable)
				{
					try
					{
						// Two independent routes, because FcFreeTypeQuery lives in the fcfreetype half
						// of libfontconfig and is not guaranteed to be exported by every build.
						family = QueryFamilyByFreeType(filePath) ?? QueryFamilyFromAppFontSet(filePath);
					}
					catch (Exception ex) when (IsMissingNativeSymbol(ex))
					{
						Debug.WriteLine($"libfontconfig unavailable, cannot read the family name of '{filePath}': {ex.Message}");
						_fontConfigUnavailable = true;
					}
				}

				// no fontconfig (or it could not read the file): parse the name table ourselves
				if (string.IsNullOrWhiteSpace(family))
					family = FontFileNames.TryReadFamilyName(filePath);

				if (string.IsNullOrWhiteSpace(family))
					family = null;

				_familyNames[filePath] = family;
				return family;
			}
		}

		/// <summary>Reads the family name straight out of the file, without adding it anywhere.</summary>
		static string QueryFamilyByFreeType(string filePath)
		{
			var pattern = IntPtr.Zero;

			try
			{
				pattern = FcFreeTypeQuery(ToUtf8(filePath), 0, IntPtr.Zero, out _);

				if (pattern == IntPtr.Zero)
					return null;

				return GetPatternString(pattern, FcFamily);
			}
			catch (EntryPointNotFoundException)
			{
				// Not fatal: the app font set route below only needs core fontconfig symbols.
				return null;
			}
			finally
			{
				if (pattern != IntPtr.Zero)
					FcPatternDestroy(pattern);
			}
		}

		/// <summary>
		/// Finds the pattern fontconfig itself built for a file we handed to
		/// <c>FcConfigAppFontAddFile</c>, and reads its family name.
		/// </summary>
		static string QueryFamilyFromAppFontSet(string filePath)
		{
			var fontSet = FcConfigGetFonts(IntPtr.Zero, FcSetApplication);

			if (fontSet == IntPtr.Zero)
				return null;

			// struct FcFontSet { int nfont; int sfont; FcPattern **fonts; }
			// The pointer sits at offset 8 on both 32- and 64-bit: two ints, then natural
			// alignment (which is a no-op at 4-byte pointers and adds nothing at 8).
			const int FontsOffset = 8;

			var count = Marshal.ReadInt32(fontSet, 0);
			var fonts = Marshal.ReadIntPtr(fontSet, FontsOffset);

			if (count <= 0 || fonts == IntPtr.Zero)
				return null;

			for (var i = 0; i < count; i++)
			{
				var pattern = Marshal.ReadIntPtr(fonts, i * IntPtr.Size);

				if (pattern == IntPtr.Zero)
					continue;

				if (string.Equals(GetPatternString(pattern, FcFile), filePath, StringComparison.Ordinal))
					return GetPatternString(pattern, FcFamily);
			}

			return null;
		}

		static string GetPatternString(IntPtr pattern, string element)
			=> FcPatternGetString(pattern, ToUtf8(element), 0, out var value) == FcResultMatch
				? FromUtf8(value)
				: null;

		/// <summary>
		/// Tells the default Pango font map that the fontconfig configuration changed, so that font
		/// maps created before the font was added pick it up. Only meaningful for the fontconfig
		/// backed font map (i.e. Linux/X11/Wayland).
		/// </summary>
		static void FlushPangoCaches()
		{
			lock (_sync)
			{
				if (_pangoFlushUnavailable)
					return;
			}

			try
			{
				var fontMap = pango_cairo_font_map_get_default();

				// the win32 / CoreText font maps are not fontconfig backed: nothing to flush
				if (fontMap != IntPtr.Zero && IsFcFontMap(fontMap))
					pango_fc_font_map_config_changed(fontMap);
			}
			catch (Exception ex) when (IsMissingNativeSymbol(ex))
			{
				// pangoft2 is not present (or the font map is not fontconfig backed). The font was
				// still added to the fontconfig configuration; font maps created later will see it.
				Debug.WriteLine($"Could not flush the Pango font cache: {ex.Message}");

				lock (_sync)
					_pangoFlushUnavailable = true;
			}
		}

		static bool IsMissingNativeSymbol(Exception ex)
			=> ex is DllNotFoundException || ex is EntryPointNotFoundException || ex is TypeLoadException;

		static byte[] ToUtf8(string value)
		{
			var bytes = Encoding.UTF8.GetBytes(value);
			var terminated = new byte[bytes.Length + 1];
			Array.Copy(bytes, terminated, bytes.Length);

			return terminated;
		}

		static string FromUtf8(IntPtr value)
		{
			if (value == IntPtr.Zero)
				return null;

			var length = 0;
			while (Marshal.ReadByte(value, length) != 0)
				length++;

			if (length == 0)
				return null;

			var bytes = new byte[length];
			Marshal.Copy(value, bytes, 0, length);

			return Encoding.UTF8.GetString(bytes);
		}
	}
}
