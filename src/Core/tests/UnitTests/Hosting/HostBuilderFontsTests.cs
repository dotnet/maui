using System;
using System.IO;
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

		[Fact]
		public void NativeFontStaysUnresolvedWithoutABackend()
		{
			// Stock Standard behaviour: with no provider installed, a font file sitting next to the app is not looked up at all.
			Assert.Null(FontRegistrar.NativeFontStreamProvider);

			var filename = $"Native{Guid.NewGuid():N}.ttf";
			var path = Path.Combine(AppContext.BaseDirectory, filename);
			File.WriteAllBytes(path, new byte[] { 1, 2, 3, 4 });

			try
			{
				var loader = new RecordingFontLoader();
				var registrar = new FontRegistrar(loader);
				registrar.Register(filename, "Native");

				Assert.Null(registrar.GetFont("Native"));
				Assert.Null(loader.FontName);
			}
			finally
			{
				File.Delete(path);
			}
		}

		[Fact]
		public void NativeFontIsHandedToTheLoaderFromTheInstalledProvider()
		{
			var old = FontRegistrar.NativeFontStreamProvider;
			string asked = null;
			FontRegistrar.NativeFontStreamProvider = name =>
			{
				asked = name;
				return new MemoryStream(new byte[] { 1, 2, 3, 4 });
			};

			try
			{
				var loader = new RecordingFontLoader();
				var registrar = new FontRegistrar(loader);
				registrar.Register("Some.ttf", "Native");

				Assert.Equal("loaded:Some.ttf", registrar.GetFont("Native"));
				Assert.Equal("Some.ttf", asked);
				Assert.Equal(new byte[] { 1, 2, 3, 4 }, loader.Content);
			}
			finally
			{
				FontRegistrar.NativeFontStreamProvider = old;
			}
		}

		[Fact]
		public void NativeFontThatTheProviderDoesNotHaveStaysUnresolved()
		{
			var old = FontRegistrar.NativeFontStreamProvider;
			FontRegistrar.NativeFontStreamProvider = _ => null;

			try
			{
				var loader = new RecordingFontLoader();
				var registrar = new FontRegistrar(loader);
				registrar.Register("NoSuchFont.ttf", "Missing");

				Assert.Null(registrar.GetFont("Missing"));
				Assert.Null(loader.FontName);
			}
			finally
			{
				FontRegistrar.NativeFontStreamProvider = old;
			}
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