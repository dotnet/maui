using System;
using System.IO;
using Microsoft.Extensions.DependencyInjection;
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
			var host = new AppHostBuilder()
				.ConfigureFonts()
				.Build();

			var manager = host.Services.GetRequiredService<IFontManager>();
			Assert.NotNull(manager);

			var registrar = host.Services.GetRequiredService<IFontRegistrar>();
			Assert.NotNull(registrar);

			var loader = host.Services.GetRequiredService<IEmbeddedFontLoader>();
			Assert.NotNull(loader);
		}

		[Theory]
		[InlineData("Dokdo-Regular.ttf", "Dokdo")]
		[InlineData("Dokdo-Regular.ttf", null)]
		public void ConfigureFontsRegistersFonts(string filename, string alias)
		{
			var root = Path.Combine(Path.GetTempPath(), "Microsoft.Maui.UnitTests", "ConfigureFontsRegistersFonts", Guid.NewGuid().ToString());

			var host = new AppHostBuilder()
				.ConfigureFonts((_, fonts) => fonts.AddEmeddedResourceFont(GetType().Assembly, filename, alias))
				.ConfigureServices((_, services) => services.AddSingleton<IEmbeddedFontLoader>(_ => new FileSystemEmbeddedFontLoader(root)))
				.Build();

			var registrar = host.Services.GetRequiredService<IFontRegistrar>();

			var path = registrar.GetFont(filename);
			Assert.NotNull(path);
			Assert.StartsWith(root, path);

			if (alias != null)
			{
				path = registrar.GetFont(alias);
				Assert.NotNull(path);
				Assert.StartsWith(root, path);
			}

			Assert.True(File.Exists(Path.Combine(root, filename)));

			Directory.Delete(root, true);
		}

		[Fact]
		public void NullAssemblyForEmbeddedFontThrows()
		{
			var builder = new AppHostBuilder()
				.ConfigureFonts((_, fonts) => fonts.AddEmeddedResourceFont(null, "test.ttf"));

			var ex = Assert.Throws<ArgumentNullException>(() => builder.Build());
			Assert.Equal("assembly", ex.ParamName);
		}

		[Theory]
		[InlineData(null)]
		[InlineData("")]
		[InlineData(" ")]
		public void BadFileNameForEmbeddedFontThrows(string filename)
		{
			var builder = new AppHostBuilder()
				.ConfigureFonts((_, fonts) => fonts.AddEmeddedResourceFont(GetType().Assembly, filename));

			var ex = Assert.ThrowsAny<ArgumentException>(() => builder.Build());
			Assert.Equal("filename", ex.ParamName);

			if (filename == null)
				Assert.IsType<ArgumentNullException>(ex);
		}

		[Fact]
		public void NullAliasForEmbeddedFontDoesNotThrow()
		{
			new AppHostBuilder()
				.ConfigureFonts((_, fonts) => fonts.AddEmeddedResourceFont(GetType().Assembly, "test.ttf", null))
				.Build();
		}
	}
}