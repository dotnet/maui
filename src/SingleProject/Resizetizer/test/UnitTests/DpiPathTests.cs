using Xunit;

namespace Microsoft.Maui.Resizetizer.Tests
{
	public class DpiPathTests
	{
		public class GetOriginal
		{
			[Theory]
			[InlineData("android", "drawable")]
			[InlineData("ios", "Resources")]
			[InlineData("uwp", "")]
			[InlineData("wpf", "")]
			public void ReturnsCorrectFolder(string platform, string folder)
			{
				var paths = DpiPath.GetOriginal(platform);

				Assert.Equal(folder, paths.Path);
			}

			[Theory]
			[InlineData("android", 1)]
			[InlineData("ios", 1)]
			[InlineData("uwp", 1)]
			[InlineData("wpf", 4)]
			public void ReturnsCorrectScale(string platform, decimal scale)
			{
				var paths = DpiPath.GetOriginal(platform);

				Assert.Equal(scale, paths.Scale);
			}

			[Theory]
			[InlineData("ANDROID")]
			[InlineData("android")]
			[InlineData("Android")]
			[InlineData("aNDROID")]
			[InlineData("AnDrOiD")]
			public void MatchesAnyCase(string platform)
			{
				var paths = DpiPath.GetOriginal(platform);

				Assert.NotNull(paths);
			}

			[Theory]
			[InlineData("macos-appkit")]
			[InlineData("gtk")]
			[InlineData("custom-platform")]
			public void ReturnsFallbackForUnknownPlatform(string platform)
			{
				var path = DpiPath.GetOriginal(platform);

				Assert.Equal("", path.Path);
				Assert.Equal(1.0m, path.Scale);
			}
		}

		public class GetDpis
		{
			[Theory]
			[InlineData("macos-appkit")]
			[InlineData("gtk")]
			[InlineData("custom-platform")]
			public void ReturnsGenericDesktopFallback(string platform)
			{
				var paths = DpiPath.GetDpis(platform);

				Assert.Collection(paths,
					path => Assert.Equal(1.0m, path.Scale),
					path =>
					{
						Assert.Equal(2.0m, path.Scale);
						Assert.Equal("@2x", path.ScaleSuffix);
					});
			}
		}

		public class GetAppIconDpis
		{
			[Theory]
			[InlineData("macos-appkit")]
			[InlineData("gtk")]
			[InlineData("custom-platform")]
			public void ReturnsGenericDesktopFallback(string platform)
			{
				var paths = DpiPath.GetAppIconDpis(platform, "appicon");

				Assert.Collection(paths,
					path => Assert.Equal(16, path.Size!.Value.Width),
					path => Assert.Equal(32, path.Size!.Value.Width),
					path => Assert.Equal(48, path.Size!.Value.Width),
					path => Assert.Equal(128, path.Size!.Value.Width),
					path => Assert.Equal(256, path.Size!.Value.Width),
					path => Assert.Equal(512, path.Size!.Value.Width),
					path => Assert.Equal(1024, path.Size!.Value.Width));
			}
		}
	}
}
