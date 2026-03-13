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
			public void ReturnsDefaultFallbackForUnknownPlatform(string platform)
			{
				var path = DpiPath.GetOriginal(platform);

				Assert.NotNull(path);
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
			public void ReturnsDefaultFallbackForUnknownPlatform(string platform)
			{
				var dpis = DpiPath.GetDpis(platform);

				Assert.NotNull(dpis);
				Assert.Equal(2, dpis.Length);
				Assert.Equal(1.0m, dpis[0].Scale);
				Assert.Equal(2.0m, dpis[1].Scale);
				Assert.Equal("@2x", dpis[1].ScaleSuffix);
			}

			[Theory]
			[InlineData("android", 5)]
			[InlineData("ios", 3)]
			[InlineData("uwp", 5)]
			public void ReturnsCorrectCountForKnownPlatform(string platform, int count)
			{
				var dpis = DpiPath.GetDpis(platform);

				Assert.Equal(count, dpis.Length);
			}
		}

		public class GetAppIconDpis
		{
			[Theory]
			[InlineData("macos-appkit")]
			[InlineData("gtk")]
			[InlineData("custom-platform")]
			public void ReturnsDefaultFallbackForUnknownPlatform(string platform)
			{
				var dpis = DpiPath.GetAppIconDpis(platform, "appicon");

				Assert.NotNull(dpis);
				Assert.Equal(7, dpis.Length);
				Assert.Equal(16, dpis[0].Size.Value.Width);
				Assert.Equal(1024, dpis[6].Size.Value.Width);
			}
		}
	}
}
