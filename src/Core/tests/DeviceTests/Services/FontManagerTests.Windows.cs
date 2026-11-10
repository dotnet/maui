using System;
using System.IO;
using System.Reflection;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Maui.Storage;
using Microsoft.UI.Xaml.Controls;
using Xunit;

namespace Microsoft.Maui.DeviceTests;

public partial class FontManagerTests : TestBase
{
	[Theory]
	[InlineData("dokdo_regular")]
	[InlineData("dokdo_regular.ttf")]
	[InlineData("dokdo_regular#dokdo")]
	[InlineData("dokdo_regular.ttf#dokdo")]
	[InlineData("myalias")]
	public Task CanLoadFonts(string fontName) =>
		InvokeOnMainThreadAsync(() =>
		{
			var registrar = new FontRegistrar(fontLoader: null);
			registrar.Register("dokdo_regular.ttf", "myalias");
			var manager = new FontManager(registrar);

			var actual = manager.GetFontFamily(Font.OfSize(fontName, 12, FontWeight.Regular));

			// Both packaged and unpackaged scenarios should work using the same URI
			// since the WinUI framework knows the difference and correctly maps the
			// URIs to the correct actual font file.
			var expected = "ms-appx:///dokdo_regular.ttf#Dokdo";

			Assert.Equal(expected, actual.Source);
		});

	[Theory]
	[InlineData("dokdo_regular")]
	[InlineData("dokdo_regular.ttf")]
	[InlineData("dokdo_regular#dokdo")]
	[InlineData("dokdo_regular.ttf#dokdo")]
	[InlineData("myalias")]
	public Task CanLoadEmbeddedFont(string fontName) =>
		InvokeOnMainThreadAsync(() =>
		{
			var registrar = new FontRegistrar(new EmbeddedFontLoader());
			registrar.Register("dokdo_regular.ttf", "myalias", GetType().Assembly);
			var manager = new FontManager(registrar);

			var actual = manager.GetFontFamily(Font.OfSize(fontName, 12, FontWeight.Regular));

			// Packaged and unpackaged scenarios do NOT work with the same URI because we are
			// storing the temporary file in a different location for unpackaged apps.

#if UNPACKAGED
			var expectedSuffix = "Fonts\\dokdo_regular.ttf#Dokdo";
			var root = Path.GetFullPath(Path.Combine(FileSystem.CacheDirectory, ".."));
			var fullFontName = Path.Combine(root, expectedSuffix);
			fullFontName = Path.GetFullPath(fullFontName);

			var filename = fullFontName[..fullFontName.IndexOf("#", StringComparison.OrdinalIgnoreCase)];
			Assert.True(File.Exists(filename), $"File not found: {filename}");
			
			var expected = "ms-appx:///" + fullFontName.Replace("\\", "/", StringComparison.OrdinalIgnoreCase);
#else
			var expected = "ms-appdata:///temp/Fonts/dokdo_regular.ttf#Dokdo";
#endif

			Assert.Equal(expected, actual.Source);
		});

	[Theory]
	[InlineData("Segoe MDL2 Assets")]
	[InlineData("Segoe UI")]
	public Task SystemFontDoesNotLogMissingFallbackFiles(string fontName) =>
		InvokeOnMainThreadAsync(() =>
		{
			var logger = new FontManagerLogger();
			using var services = new ServiceCollection()
				.AddSingleton<ILogger<FontManager>>(logger)
				.BuildServiceProvider();
			var manager = new FontManager(new FontRegistrar(fontLoader: null), services);

			var actual = manager.GetFontFamily(Font.OfSize(fontName, 30));

			Assert.StartsWith(fontName + ", ", actual.Source, StringComparison.Ordinal);
			Assert.Equal(0, logger.ErrorCount);
		});

	[Fact]
	public Task CanReadFamilyNameFromRelativeFontFile() =>
		InvokeOnMainThreadAsync(() =>
		{
			var manager = new FontManager(new RelativeFontRegistrar());

			var actual = manager.GetFontFamily(Font.OfSize("relative-font", 12));

			Assert.Equal("dokdo_regular.ttf#Dokdo", actual.Source);
		});

	sealed class RelativeFontRegistrar : IFontRegistrar
	{
		public string GetFont(string font) => "dokdo_regular.ttf";

		public void Register(string filename, string alias) => throw new NotSupportedException();

		public void Register(string filename, string alias, Assembly assembly) => throw new NotSupportedException();
	}

	sealed class FontManagerLogger : ILogger<FontManager>
	{
		public int ErrorCount { get; private set; }

		public IDisposable BeginScope<TState>(TState state) => null;

		public bool IsEnabled(LogLevel logLevel) => true;

		public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception exception, Func<TState, Exception, string> formatter)
		{
			if (logLevel >= LogLevel.Error)
				ErrorCount++;
		}
	}
}
