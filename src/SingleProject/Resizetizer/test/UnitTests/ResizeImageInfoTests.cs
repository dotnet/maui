using System.Collections.Generic;
using System.IO;
using Microsoft.Build.Utilities;
using SkiaSharp;
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

#pragma warning disable CS0618 // Type or member is obsolete
		public class FilterQualityTests
		{
			[Fact]
			public void DefaultFilterQualityIsHigh()
			{
				var info = new ResizeImageInfo();
				Assert.Equal(SKFilterQuality.High, info.FilterQuality);
			}

			[Fact]
			public void DefaultFilterQualityConstantIsHigh()
			{
				Assert.Equal(SKFilterQuality.High, ResizeImageInfo.DefaultFilterQuality);
			}

			[Theory]
			[InlineData(SKFilterQuality.None)]
			[InlineData(SKFilterQuality.Low)]
			[InlineData(SKFilterQuality.Medium)]
			[InlineData(SKFilterQuality.High)]
			public void FilterQualityCanBeSet(SKFilterQuality quality)
			{
				var info = new ResizeImageInfo
				{
					FilterQuality = quality
				};

				Assert.Equal(quality, info.FilterQuality);
			}

			[Theory]
			[InlineData("None", SKFilterQuality.None)]
			[InlineData("Low", SKFilterQuality.Low)]
			[InlineData("Medium", SKFilterQuality.Medium)]
			[InlineData("High", SKFilterQuality.High)]
			public void FilterQualityParsedFromTaskItem(string metadataValue, SKFilterQuality expected)
			{
				var path = Path.GetFullPath("images/camera.png");
				var item = new TaskItem(path, new Dictionary<string, string>
				{
					["FilterQuality"] = metadataValue
				});

				var info = ResizeImageInfo.Parse(item);
				Assert.Equal(expected, info.FilterQuality);
			}

			[Fact]
			public void FilterQualityDefaultsToHighWhenNotSpecified()
			{
				var path = Path.GetFullPath("images/camera.png");
				var item = new TaskItem(path);

				var info = ResizeImageInfo.Parse(item);
				Assert.Equal(SKFilterQuality.High, info.FilterQuality);
			}

			[Fact]
			public void FilterQualityDefaultsToHighForInvalidValue()
			{
				var path = Path.GetFullPath("images/camera.png");
				var item = new TaskItem(path, new Dictionary<string, string>
				{
					["FilterQuality"] = "InvalidValue"
				});

				var info = ResizeImageInfo.Parse(item);
				Assert.Equal(SKFilterQuality.High, info.FilterQuality);
			}
		}
#pragma warning restore CS0618 // Type or member is obsolete
	}
}
