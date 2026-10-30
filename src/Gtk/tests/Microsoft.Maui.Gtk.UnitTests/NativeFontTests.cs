using System;
using System.IO;
using System.Linq;
using Microsoft.Maui.Gtk.Fonts;
using Xunit;

namespace Microsoft.Maui.Gtk.UnitTests
{
	/// <summary>Fonts registered by file name are read from the folders MauiFont copies them to; no GTK needed.</summary>
	public class NativeFontTests
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

		[Fact]
		public void MissingNativeFontIsNull() => Assert.Null(GtkEmbeddedFontLoader.OpenNativeFont("NoSuchFont.ttf"));

		[Fact]
		public void NativeFontWithADirectoryInItsNameLooksOnlyAtTheFileName() =>
			Assert.Null(GtkEmbeddedFontLoader.OpenNativeFont("../../etc/passwd"));
	}
}
