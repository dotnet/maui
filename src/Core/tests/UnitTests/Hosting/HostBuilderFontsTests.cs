using System;
using System.IO;
using System.Linq;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Maui.Graphics;
using Microsoft.Maui.Hosting;
using Xunit;

namespace Microsoft.Maui.UnitTests.Hosting
{
	[Category(TestCategory.Core, TestCategory.Hosting)]
	public class HostBuilderFontsTests
	{
		[Fact]
		public void ConfigureFontsRegistersTheCorrectServices()
		{
			var builder = MauiApp
				.CreateBuilder()
				.ConfigureFonts();
			var mauiApp = builder.Build();

			var manager = mauiApp.Services.GetRequiredService<IFontManager>();
			Assert.NotNull(manager);

			var registrar = mauiApp.Services.GetRequiredService<IFontRegistrar>();
			Assert.NotNull(registrar);

			var loader = mauiApp.Services.GetRequiredService<IEmbeddedFontLoader>();
			Assert.NotNull(loader);
		}

		[Theory]
		[InlineData("Dokdo-Regular.ttf", "Dokdo")]
		[InlineData("Dokdo-Regular.ttf", null)]
		public void ConfigureFontsRegistersFonts(string filename, string alias)
		{
			var root = Path.Combine(Path.GetTempPath(), "Microsoft.Maui.UnitTests", "ConfigureFontsRegistersFonts", Guid.NewGuid().ToString());

			var builder = MauiApp
				.CreateBuilder()
				.ConfigureFonts(fonts => fonts.AddEmbeddedResourceFont(GetType().Assembly, filename, alias));
			builder.Services.AddSingleton<IEmbeddedFontLoader>(_ => new FileSystemEmbeddedFontLoader(root));
			var mauiApp = builder.Build();

			var registrar = mauiApp.Services.GetRequiredService<IFontRegistrar>();

			var path = registrar.GetFont(filename);
			Assert.NotNull(path);
			Assert.StartsWith(root, path, StringComparison.Ordinal);

			if (alias != null)
			{
				path = registrar.GetFont(alias);
				Assert.NotNull(path);
				Assert.StartsWith(root, path, StringComparison.Ordinal);
			}

			Assert.True(File.Exists(Path.Combine(root, filename)));

			Directory.Delete(root, true);
		}

		[Fact]
		public void NullAssemblyForEmbeddedFontThrows()
		{
			var builder = MauiApp
				.CreateBuilder()
				.ConfigureFonts(fonts => fonts.AddEmbeddedResourceFont(null, "test.ttf"));

			var ex = Assert.Throws<ArgumentNullException>(() => builder.Build());
			Assert.Equal("assembly", ex.ParamName);
		}

		[Theory]
		[InlineData(null)]
		[InlineData("")]
		[InlineData(" ")]
		public void BadFileNameForEmbeddedFontThrows(string filename)
		{
			var builder = MauiApp
				.CreateBuilder()
				.ConfigureFonts(fonts => fonts.AddEmbeddedResourceFont(GetType().Assembly, filename));

			var ex = Assert.ThrowsAny<ArgumentException>(() => builder.Build());
			Assert.Equal("filename", ex.ParamName);

			if (filename == null)
				Assert.IsType<ArgumentNullException>(ex);
		}

		[Fact]
		public void NullAliasForEmbeddedFontDoesNotThrow()
		{
			var builder = MauiApp
				.CreateBuilder()
				.ConfigureFonts(fonts => fonts.AddEmbeddedResourceFont(GetType().Assembly, "test.ttf", null));

			_ = builder.Build();
		}

		[Theory]
		[InlineData("Resources", "Fonts")]
		[InlineData("Fonts", null)]
		[InlineData(null, null)]
		public void NativeFontIsHandedToTheLoaderFromTheFolderNextToTheApp(string folder1, string folder2)
		{
			var folder = Path.Combine(AppContext.BaseDirectory, Path.Combine(new[] { folder1, folder2 }.Where(f => f != null).ToArray()));
			var filename = $"Native{Guid.NewGuid():N}.ttf";
			Directory.CreateDirectory(folder);
			File.WriteAllBytes(Path.Combine(folder, filename), new byte[] { 1, 2, 3, 4 });

			try
			{
				var loader = new RecordingFontLoader();
				var registrar = new FontRegistrar(loader);
				registrar.Register(filename, "Native");

				var result = registrar.GetFont("Native");

				Assert.Equal("loaded:" + filename, result);
				Assert.Equal(filename, loader.FontName);
				Assert.Equal(new byte[] { 1, 2, 3, 4 }, loader.Content);
			}
			finally
			{
				File.Delete(Path.Combine(folder, filename));
			}
		}

		[Fact]
		public void NativeFontThatIsNotThereStaysUnresolved()
		{
			var loader = new RecordingFontLoader();
			var registrar = new FontRegistrar(loader);
			registrar.Register("NoSuchFont.ttf", "Missing");

			Assert.Null(registrar.GetFont("Missing"));
			Assert.Null(loader.FontName);
		}

		[Fact]
		public void NativeFontWithADirectoryInItsNameLooksOnlyAtTheFileName()
		{
			var loader = new RecordingFontLoader();
			var registrar = new FontRegistrar(loader);
			registrar.Register("../../etc/passwd", "Evil");

			Assert.Null(registrar.GetFont("Evil"));
			Assert.Null(loader.FontName);
		}

		sealed class RecordingFontLoader : IEmbeddedFontLoader
		{
			public string FontName { get; private set; }

			public byte[] Content { get; private set; }

			public string LoadFont(EmbeddedFont font)
			{
				FontName = font.FontName;
				using var memory = new MemoryStream();
				font.ResourceStream.CopyTo(memory);
				Content = memory.ToArray();
				return "loaded:" + font.FontName;
			}
		}
	}
}