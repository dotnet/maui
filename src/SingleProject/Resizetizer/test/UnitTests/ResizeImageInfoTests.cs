using System.Collections.Generic;
using System;
using Microsoft.Build.Utilities;
using SkiaSharp;
using Xunit;

namespace Microsoft.Maui.Resizetizer.Tests
{
	public class ResizeImageInfoTests
	{
		public class Parse
		{
			[Fact]
			public void SupportsDarkSplashMetadata()
			{
				var image = new TaskItem("images/camera.png", new Dictionary<string, string>
				{
					["DarkFile"] = "images/camera_color.png",
					["DarkColor"] = "#000000",
					["DarkTintColor"] = "#ffffff",
				});

				var info = ResizeImageInfo.Parse(image);

				Assert.Equal(SKColors.Black, info.DarkColor);
				Assert.Equal(SKColors.White, info.DarkTintColor);
				Assert.EndsWith("camera_color.png", info.DarkFilename, StringComparison.Ordinal);
				Assert.False(info.DarkIsVector);
			}

			[Fact]
			public void DarkTintColorFallsBackToTintColorOnlyWhenDarkFileIsNotSpecified()
			{
				var image = new TaskItem("images/camera.png", new Dictionary<string, string>
				{
					["TintColor"] = "#ff0000",
				});

				var info = ResizeImageInfo.Parse(image);
				var darkInfo = info.CreateDarkVariant();

				Assert.Equal(SKColors.Red, darkInfo.TintColor);
				Assert.Equal(info.Filename, darkInfo.Filename);
			}

			[Fact]
			public void DarkTintColorDoesNotFallbackToTintColorWhenDarkFileIsSpecified()
			{
				var image = new TaskItem("images/camera.png", new Dictionary<string, string>
				{
					["TintColor"] = "#ff0000",
					["DarkFile"] = "images/camera_color.png",
				});

				var info = ResizeImageInfo.Parse(image);
				var darkInfo = info.CreateDarkVariant();

				Assert.Null(darkInfo.TintColor);
				Assert.EndsWith("camera_color.png", darkInfo.Filename, StringComparison.Ordinal);
			}
		}

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
	}
}
