using System;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using Microsoft.Maui.Gtk.Fonts;
using Xunit;

namespace Microsoft.Maui.Gtk.UnitTests
{
	/// <summary>Fonts registered by file name are read from the folders MauiFont copies them to; no GTK needed.</summary>
	public unsafe class NativeFontTests
	{
		[Theory]
		[InlineData("Resources", "Fonts")]
		[InlineData("Fonts", null)]
		[InlineData(null, null)]
		public void NativeFontIsOpenedFromTheFolderNextToTheApp(string? folder1, string? folder2)
		{
			var folder = Path.Combine(AppContext.BaseDirectory, Path.Combine(new[] { folder1, folder2 }.Where(f => f != null).Select(f => f!).ToArray()));
			var filename = $"Native{Guid.NewGuid():N}.ttf";
			Directory.CreateDirectory(folder);
			File.WriteAllBytes(Path.Combine(folder, filename), new byte[] { 1, 2, 3, 4 });

			try
			{
				using var stream = GtkEmbeddedFontLoader.OpenNativeFont(filename);
				Assert.NotNull(stream);
				Assert.Equal(4, stream.Length);
			}
			finally
			{
				File.Delete(Path.Combine(folder, filename));
			}
		}

		static string FeatherFont => Path.Combine(AppContext.BaseDirectory, "Fonts", "feather.ttf");

		[Fact]
		public void FamilyNameIsReadFromTheFontFileWithoutAnyNativeLibrary() =>
			Assert.Equal("feather", FontFileNames.TryReadFamilyName(FeatherFont));

		[Fact]
		public void FamilyNameOfAMissingOrBrokenFileIsNull()
		{
			Assert.Null(FontFileNames.TryReadFamilyName(Path.Combine(AppContext.BaseDirectory, "no-such.ttf")));
			Assert.Null(FontFileNames.ReadFamilyName(new byte[] { 1, 2, 3, 4 }));
			Assert.Null(FontFileNames.ReadFamilyName(new byte[64]));
		}

		/// <summary>A minimal sfnt: offset table, one <c>name</c> record and the name table itself.</summary>
		static byte[] BuildSfnt(params (ushort Platform, ushort Language, ushort NameId, byte[] Text)[] names)
		{
			var storageOffset = 6 + 12 * names.Length;
			var table = new System.Collections.Generic.List<byte>();
			void U16(System.Collections.Generic.List<byte> l, int v) { l.Add((byte)(v >> 8)); l.Add((byte)v); }
			U16(table, 0); U16(table, names.Length); U16(table, storageOffset);
			var strings = new System.Collections.Generic.List<byte>();
			foreach (var n in names)
			{
				U16(table, n.Platform); U16(table, 0); U16(table, n.Language); U16(table, n.NameId);
				U16(table, n.Text.Length); U16(table, strings.Count);
				strings.AddRange(n.Text);
			}
			table.AddRange(strings);

			var file = new System.Collections.Generic.List<byte>();
			file.AddRange(new byte[] { 0, 1, 0, 0 }); U16(file, 1); file.AddRange(new byte[6]);
			file.AddRange(System.Text.Encoding.ASCII.GetBytes("name")); file.AddRange(new byte[4]);
			file.AddRange(new byte[] { 0, 0, 0, 28 });
			file.AddRange(new byte[] { 0, 0, (byte)(table.Count >> 8), (byte)table.Count });
			file.AddRange(table);
			return file.ToArray();
		}

		static byte[] Utf16(string s) => System.Text.Encoding.BigEndianUnicode.GetBytes(s);

		[Fact]
		public void TypographicFamilyIsPreferredOverFamilyName() =>
			Assert.Equal("Family", FontFileNames.ReadFamilyName(BuildSfnt(
				(3, 0x0409, 1, Utf16("Family SemiBold")),
				(3, 0x0409, 16, Utf16("Family")))));

		[Fact]
		public void TypographicFamilyIsPreferredRegardlessOfRecordOrder() =>
			Assert.Equal("Family", FontFileNames.ReadFamilyName(BuildSfnt(
				(3, 0x0409, 16, Utf16("Family")),
				(3, 0x0409, 1, Utf16("Family SemiBold")))));

		[Fact]
		public void FamilyNameIsUsedWhenThereIsNoTypographicFamily() =>
			Assert.Equal("Family Bold", FontFileNames.ReadFamilyName(BuildSfnt(
				(3, 0x0409, 2, Utf16("Regular")),
				(3, 0x0409, 1, Utf16("Family Bold")))));

		[Fact]
		public void EnglishWindowsNameIsPreferredOverMacRoman()
		{
			Assert.Equal("WinName", FontFileNames.ReadFamilyName(BuildSfnt(
				(1, 0, 1, System.Text.Encoding.ASCII.GetBytes("MacName")),
				(3, 0x0409, 1, Utf16("WinName")))));
			Assert.Equal("MacName", FontFileNames.ReadFamilyName(BuildSfnt(
				(3, 0x0407, 1, Utf16("DeutschName")),
				(1, 0, 1, System.Text.Encoding.ASCII.GetBytes("MacName")))));
		}

		[Fact]
		public void MacRomanBytesAboveAsciiAreDecoded()
		{
			// 0x8E = e acute, 0x9F = u diaeresis, 0xA5 = bullet, 0xD2/0xD3 = curly double quotes, 0xDB = euro
			Assert.Equal("Café", FontFileNames.ReadFamilyName(BuildSfnt((1, 0, 1, new byte[] { (byte)'C', (byte)'a', (byte)'f', 0x8E }))));
			Assert.Equal("ü•“”€", FontFileNames.ReadFamilyName(BuildSfnt((1, 0, 1, new byte[] { 0x9F, 0xA5, 0xD2, 0xD3, 0xDB }))));
		}

		[Fact]
		public void TruncatedOrMalformedNameTablesReturnNull()
		{
			var good = BuildSfnt((3, 0x0409, 1, Utf16("Family")));
			Assert.Equal("Family", FontFileNames.ReadFamilyName(good));

			for (var length = 0; length < good.Length; length++)
			{
				var truncated = good.Take(length).ToArray();
				var ex = Record.Exception(() => FontFileNames.ReadFamilyName(truncated));
				Assert.Null(ex);
			}

			// name table length below the 6 byte header
			var shortTable = (byte[])good.Clone();
			shortTable[24] = 0; shortTable[25] = 0; shortTable[26] = 0; shortTable[27] = 4;
			Assert.Null(FontFileNames.ReadFamilyName(shortTable));

			// string offset pointing past the end of the file
			var badString = (byte[])good.Clone();
			badString[44] = 0xFF; badString[45] = 0xFF;
			Assert.Null(FontFileNames.ReadFamilyName(badString));
		}

		/// <summary>A file Pango rejects must not leak its GError (freed with g_error_free) and must report false.</summary>
		[Fact]
		public void PangoRejectedFontFileReturnsFalseAndFreesTheError()
		{
			try
			{
				FontNativeLibraries.GetExport(FontLibrary.Pango, "pango_font_map_add_font_file");
				FontNativeLibraries.GetExport(FontLibrary.GLib, "g_error_free");
			}
			catch (Exception ex) when (ex is DllNotFoundException || ex is EntryPointNotFoundException)
			{
				return; // Pango < 1.56 (or no Pango): nothing to assert
			}

			var path = Path.Combine(Path.GetTempPath(), $"not-a-font-{Guid.NewGuid():N}.ttf");
			File.WriteAllBytes(path, new byte[] { 1, 2, 3, 4, 5, 6, 7, 8 });
			try
			{
				for (var i = 0; i < 3; i++)
					Assert.False(FontConfigHelper.TryAddFontFileWithPango(path));
			}
			finally
			{
				File.Delete(path);
			}
		}

		[Fact]
		public void FamilyNameLookupWorksOnThisOs() =>
			Assert.Equal("feather", FontConfigHelper.QueryFamilyName(FeatherFont));

		[Theory]
		[InlineData("FontConfig", "libfontconfig.so.1", "fontconfig-1.dll", "libfontconfig.1.dylib")]
		[InlineData("Pango", "libpango-1.0.so.0", "pango-1.0-0.dll", "libpango-1.0.0.dylib")]
		[InlineData("PangoCairo", "libpangocairo-1.0.so.0", "pangocairo-1.0-0.dll", "libpangocairo-1.0.0.dylib")]
		[InlineData("PangoFt2", "libpangoft2-1.0.so.0", "pangoft2-1.0-0.dll", "libpangoft2-1.0.0.dylib")]
		[InlineData("GObject", "libgobject-2.0.so.0", "gobject-2.0-0.dll", "libgobject-2.0.0.dylib")]
		[InlineData("GLib", "libglib-2.0.so.0", "glib-2.0-0.dll", "libglib-2.0.0.dylib")]
		public void NativeLibraryCandidatesAreChosenPerOs(string name, string linux, string windows, string mac)
		{
			var library = Enum.Parse<FontLibrary>(name);
			Assert.Equal(linux, FontNativeLibraries.GetCandidates(library, OSPlatform.Linux)[0]);
			Assert.Equal(windows, FontNativeLibraries.GetCandidates(library, OSPlatform.Windows)[0]);
			Assert.Equal(mac, FontNativeLibraries.GetCandidates(library, OSPlatform.OSX)[0]);
			Assert.DoesNotContain(FontNativeLibraries.GetCandidates(library, OSPlatform.Windows), n => n.EndsWith(".so.0", StringComparison.Ordinal));
		}

		/// <summary>Registering an embedded font must make Pango (the font map GTK draws with) know its family, on every OS.</summary>
		[GtkFact]
		public void EmbeddedFontIsResolvedByPangoAndTheAliasMapsToItsRealFamily() => GtkTestHost.Run(() =>
		{
			var name = $"maui-gtk-feather-{Guid.NewGuid():N}.ttf";
			string? path;
			using (var stream = File.OpenRead(FeatherFont))
				path = new GtkEmbeddedFontLoader().LoadFont(new EmbeddedFont { FontName = name, ResourceStream = stream });

			try
			{
				Assert.NotNull(path);
				Assert.Contains("feather", PangoFamilies());

				var registrar = new AliasRegistrar("FeatherAlias", path!);
				Assert.Equal("feather", new GtkFontManager(registrar).GetFontFamily("FeatherAlias"));
			}
			finally
			{
				if (path is not null)
					File.Delete(path);
			}
		});

		/// <summary>The family names of the font map GTK renders with (pango_cairo_font_map_get_default).</summary>
		static string[] PangoFamilies()
		{
			var getDefault = (delegate* unmanaged[Cdecl]<IntPtr>)FontNativeLibraries.GetExport(FontLibrary.PangoCairo, "pango_cairo_font_map_get_default");
			var listFamilies = (delegate* unmanaged[Cdecl]<IntPtr, IntPtr*, int*, void>)FontNativeLibraries.GetExport(FontLibrary.Pango, "pango_font_map_list_families");
			var familyName = (delegate* unmanaged[Cdecl]<IntPtr, IntPtr>)FontNativeLibraries.GetExport(FontLibrary.Pango, "pango_font_family_get_name");

			IntPtr families;
			int count;
			listFamilies(getDefault(), &families, &count);

			var names = new string[count];
			for (var i = 0; i < count; i++)
				names[i] = Marshal.PtrToStringUTF8(familyName(Marshal.ReadIntPtr(families, i * IntPtr.Size))) ?? string.Empty;

			// the array (not the families) belongs to the caller
			((delegate* unmanaged[Cdecl]<IntPtr, void>)FontNativeLibraries.GetExport(FontLibrary.GLib, "g_free"))(families);

			return names;
		}

		sealed class AliasRegistrar : IFontRegistrar
		{
			readonly string _alias;
			readonly string _path;

			public AliasRegistrar(string alias, string path)
			{
				_alias = alias;
				_path = path;
			}

			public void Register(string filename, string? alias, System.Reflection.Assembly assembly) { }

			public void Register(string filename, string? alias) { }

			public string? GetFont(string font) => font == _alias ? _path : null;
		}

		[Fact]
		public void MissingNativeFontIsNull() => Assert.Null(GtkEmbeddedFontLoader.OpenNativeFont("NoSuchFont.ttf"));

		[Fact]
		public void NativeFontWithADirectoryInItsNameLooksOnlyAtTheFileName() =>
			Assert.Null(GtkEmbeddedFontLoader.OpenNativeFont("../../etc/passwd"));
	}
}
