#nullable disable
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Text;

namespace Microsoft.Maui.Gtk.Fonts
{
	/// <summary>
	/// A thin P/Invoke layer over libfontconfig, used to make a font file that was written at
	/// runtime (an <see cref="EmbeddedFont"/> extracted by <see cref="GtkEmbeddedFontLoader"/>)
	/// usable by Pango <em>in this process</em>.
	/// <para>
	/// Dropping a file into <c>~/.local/share/fonts</c> is only honoured by processes that start
	/// afterwards - fontconfig builds its font set once, at first use. <c>FcConfigAppFontAddFile</c>
	/// injects the file into the current configuration immediately; the Pango cache flush that
	/// follows makes the new family visible to font maps that have already been created.
	/// </para>
	/// <para>
	/// Everything here is best-effort: if libfontconfig cannot be loaded (non-Linux, or a stripped
	/// container) the helper turns itself off and callers fall back to name-based resolution.
	/// </para>
	/// </summary>
	internal static class FontConfigHelper
	{
		// sonames, not link names: "libfontconfig.so" only exists when the -dev package is installed.
		const string FontConfigLib = "libfontconfig.so.1";
		const string PangoCairoLib = "libpangocairo-1.0.so.0";
		const string PangoFt2Lib = "libpangoft2-1.0.so.0";
		const string PangoLib = "libpango-1.0.so.0";

		const int FcResultMatch = 0;
		const int FcSetApplication = 1;

		const string FcFamily = "family";
		const string FcFile = "file";

		static readonly object _sync = new object();
		static readonly HashSet<string> _addedFiles = new HashSet<string>(StringComparer.Ordinal);
		static readonly Dictionary<string, string> _familyNames = new Dictionary<string, string>(StringComparer.Ordinal);

		static bool _fontConfigUnavailable;
		static bool _pangoFlushUnavailable;

		[DllImport(FontConfigLib, CallingConvention = CallingConvention.Cdecl)]
		static extern int FcConfigAppFontAddFile(IntPtr config, byte[] file);

		[DllImport(FontConfigLib, CallingConvention = CallingConvention.Cdecl)]
		static extern IntPtr FcFreeTypeQuery(byte[] file, uint id, IntPtr blanks, out int count);

		[DllImport(FontConfigLib, CallingConvention = CallingConvention.Cdecl)]
		static extern int FcPatternGetString(IntPtr pattern, byte[] obj, int n, out IntPtr value);

		[DllImport(FontConfigLib, CallingConvention = CallingConvention.Cdecl)]
		static extern void FcPatternDestroy(IntPtr pattern);

		[DllImport(FontConfigLib, CallingConvention = CallingConvention.Cdecl)]
		static extern IntPtr FcConfigGetFonts(IntPtr config, int set);

		[DllImport(PangoCairoLib, CallingConvention = CallingConvention.Cdecl)]
		static extern IntPtr pango_cairo_font_map_get_default();

		[DllImport(PangoFt2Lib, CallingConvention = CallingConvention.Cdecl)]
		static extern void pango_fc_font_map_config_changed(IntPtr fontMap);

		// Pango >= 1.56: loads a font file straight into a font map, without touching the fontconfig configuration.
		[DllImport(PangoLib, CallingConvention = CallingConvention.Cdecl)]
		static extern int pango_font_map_add_font_file(IntPtr fontMap, byte[] filename, out IntPtr error);

		/// <summary>
		/// Whether the fontconfig entry points could be resolved. False on Windows/macOS, or when
		/// libfontconfig.so.1 is absent - in that case the font file is still written to the user
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
				return fontMap != IntPtr.Zero && pango_font_map_add_font_file(fontMap, ToUtf8(filePath), out _) != 0;
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
		/// Asks fontconfig for the real family name inside a font file - the name Pango matches on,
		/// which is generally not the file name. Returns null when it cannot be determined.
		/// </summary>
		internal static string QueryFamilyName(string filePath)
		{
			if (string.IsNullOrEmpty(filePath))
				return null;

			lock (_sync)
			{
				if (_familyNames.TryGetValue(filePath, out var cached))
					return cached;

				if (_fontConfigUnavailable)
					return null;

				string family;

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
					return null;
				}

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

				if (fontMap != IntPtr.Zero)
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
