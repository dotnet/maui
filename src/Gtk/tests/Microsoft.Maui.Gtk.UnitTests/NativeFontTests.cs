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

		[Fact]
		public void FamilyNameLookupWorksOnThisOs() =>
			Assert.Equal("feather", FontConfigHelper.QueryFamilyName(FeatherFont));

		[Theory]
		[InlineData("FontConfig", "libfontconfig.so.1", "fontconfig-1.dll", "libfontconfig.1.dylib")]
		[InlineData("Pango", "libpango-1.0.so.0", "pango-1.0-0.dll", "libpango-1.0.0.dylib")]
		[InlineData("PangoCairo", "libpangocairo-1.0.so.0", "pangocairo-1.0-0.dll", "libpangocairo-1.0.0.dylib")]
		[InlineData("PangoFt2", "libpangoft2-1.0.so.0", "pangoft2-1.0-0.dll", "libpangoft2-1.0.0.dylib")]
		[InlineData("GObject", "libgobject-2.0.so.0", "gobject-2.0-0.dll", "libgobject-2.0.0.dylib")]
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
