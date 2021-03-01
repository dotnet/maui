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
			[InlineData("uwp", "Assets")]
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
			[InlineData("ANDROID123")]
			[InlineData("Cars")]
			[InlineData("123")]
			public void ReturnsNullOnInvalidPlatform(string platform)
			{
				var paths = DpiPath.GetOriginal(platform);

				Assert.Null(paths);
			}
		}
	}
}
