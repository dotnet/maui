using System;
using System.Collections.Generic;
using System.IO;
using Microsoft.Build.Utilities;
using Xunit;

namespace Microsoft.Maui.Resizetizer.Tests
{
	public class ResizeImageInfoTests
	{
		public class IsVector
		{
			[Theory]
			[InlineData("image.svg", true)]
			[InlineData("IMAGE.svg", true)]
			[InlineData("image.SVG", true)]
			[InlineData("IMAGE.SVG", true)]
			[InlineData("image.jpeg", false)]
			[InlineData("IMAGE.jpeg", false)]
			[InlineData("image.JPEG", false)]
			[InlineData("IMAGE.JPEG", false)]
			[InlineData("image.png", false)]
			[InlineData("IMAGE.png", false)]
			[InlineData("image.PNG", false)]
			[InlineData("IMAGE.PNG", false)]
			public void ReturnsCorrectFolder(string filename, bool isVector)
			{
				var info = new ResizeImageInfo
				{
					Filename = filename
				};

				Assert.Equal(isVector, info.IsVector);
			}

			[Theory]
			[InlineData("image")]
			[InlineData("IMAGE")]
			public void SupportsNoExtension(string filename)
			{
				var info = new ResizeImageInfo
				{
					Filename = filename
				};

				Assert.False(info.IsVector);
			}

			[Theory]
			[InlineData("")]
			[InlineData(null)]
			public void DoesNotCrashOnNullOrEmpty(string filename)
			{
				var info = new ResizeImageInfo
				{
					Filename = filename
				};

				Assert.False(info.IsVector);
			}
		}

		public class ResizeQualityTests
		{
			[Fact]
			public void DefaultQualityIsAuto()
			{
				var info = new ResizeImageInfo();
				Assert.Equal(ResizeQuality.Auto, info.Quality);
			}

			[Fact]
			public void DefaultQualityConstantIsAuto()
			{
				Assert.Equal(ResizeQuality.Auto, ResizeImageInfo.DefaultResizeQuality);
			}

			[Theory]
			[InlineData("Auto")]
			[InlineData("Best")]
			[InlineData("Fastest")]
			public void QualityCanBeSet(string qualityName)
			{
				var quality = Enum.Parse<ResizeQuality>(qualityName);
				var info = new ResizeImageInfo
				{
					Quality = quality
				};

				Assert.Equal(quality, info.Quality);
			}

			[Theory]
			[InlineData("Auto")]
			[InlineData("Best")]
			[InlineData("Fastest")]
			public void QualityParsedFromTaskItem(string metadataValue)
			{
				var expected = Enum.Parse<ResizeQuality>(metadataValue);
				var path = Path.GetFullPath("images/camera.png");
				var item = new TaskItem(path, new Dictionary<string, string>
				{
					["ResizeQuality"] = metadataValue
				});

				var info = ResizeImageInfo.Parse(item);
				Assert.Equal(expected, info.Quality);
			}

			[Fact]
			public void QualityDefaultsToAutoWhenNotSpecified()
			{
				var path = Path.GetFullPath("images/camera.png");
				var item = new TaskItem(path);

				var info = ResizeImageInfo.Parse(item);
				Assert.Equal(ResizeQuality.Auto, info.Quality);
			}

			[Fact]
			public void QualityDefaultsToAutoForInvalidValue()
			{
				var path = Path.GetFullPath("images/camera.png");
				var item = new TaskItem(path, new Dictionary<string, string>
				{
					["ResizeQuality"] = "InvalidValue"
				});

				var info = ResizeImageInfo.Parse(item);
				Assert.Equal(ResizeQuality.Auto, info.Quality);
			}

			[Theory]
			[InlineData("auto")]
			[InlineData("FASTEST")]
			[InlineData("best")]
			public void QualityParsingIsCaseInsensitive(string metadataValue)
			{
				var expected = Enum.Parse<ResizeQuality>(metadataValue, ignoreCase: true);
				var path = Path.GetFullPath("images/camera.png");
				var item = new TaskItem(path, new Dictionary<string, string>
				{
					["ResizeQuality"] = metadataValue
				});

				var info = ResizeImageInfo.Parse(item);
				Assert.Equal(expected, info.Quality);
			}
		}
	}
}
