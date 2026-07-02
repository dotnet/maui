using System.Collections.Generic;
using System;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Xml.Linq;
using Microsoft.Build.Framework;
using Microsoft.Build.Utilities;
using SkiaSharp;
using Xunit;
using Xunit.Abstractions;

namespace Microsoft.Maui.Resizetizer.Tests
{
	public class GenerateSplashAssetCatalogTests : MSBuildTaskTestFixture<GenerateSplashAssetCatalog>
	{
		public GenerateSplashAssetCatalogTests(ITestOutputHelper outputHelper)
			: base(outputHelper)
		{
		}

		protected GenerateSplashAssetCatalog GetNewTask(params ITaskItem[] splash) =>
			new()
			{
				IntermediateOutputPath = DestinationDirectory,
				InputsFile = "mauisplash.inputs",
				MauiSplashScreen = splash,
				BuildEngine = this,
			};

		[Fact]
		public void DarkMetadataGeneratesImageAndColorAssetCatalogs()
		{
			var splash = new TaskItem("images/camera.png", new Dictionary<string, string>
			{
				["Color"] = "#ffffff",
				["DarkColor"] = "#000000",
				["DarkFile"] = "images/camera_color.png",
				["BaseSize"] = "44",
			});

			var task = GetNewTask(splash);
			var success = task.Execute();
			Assert.True(success, LogErrorEvents.FirstOrDefault()?.Message);

			AssertFileExists("Assets.xcassets/MauiSplashImage.imageset/Contents.json");
			AssertFileExists("Assets.xcassets/MauiSplashColor.colorset/Contents.json");
			AssertFileSize("Assets.xcassets/MauiSplashImage.imageset/MauiSplashImage.png", 44, 44);
			AssertFileSize("Assets.xcassets/MauiSplashImage.imageset/MauiSplashImage@2x.png", 88, 88);
			AssertFileSize("Assets.xcassets/MauiSplashImage.imageset/MauiSplashImageDark.png", 44, 44);

			using var imageJson = JsonDocument.Parse(File.ReadAllText(Path.Combine(DestinationDirectory, "Assets.xcassets", "MauiSplashImage.imageset", "Contents.json")));
			var images = imageJson.RootElement.GetProperty("images").EnumerateArray().ToArray();
			Assert.Contains(images, image => !image.TryGetProperty("appearances", out _));
			Assert.Contains(images, image => GetAppearanceValue(image) == "light");
			Assert.Contains(images, image => GetAppearanceValue(image) == "dark");

			using var colorJson = JsonDocument.Parse(File.ReadAllText(Path.Combine(DestinationDirectory, "Assets.xcassets", "MauiSplashColor.colorset", "Contents.json")));
			var colors = colorJson.RootElement.GetProperty("colors").EnumerateArray().ToArray();
			Assert.Contains(colors, color => !color.TryGetProperty("appearances", out _));
			Assert.Contains(colors, color => GetAppearanceValue(color) == "light");
			var darkColor = Assert.Single(colors.Where(color => GetAppearanceValue(color) == "dark"));
			Assert.Equal("0", darkColor.GetProperty("color").GetProperty("components").GetProperty("red").GetString());
			Assert.Equal("0", darkColor.GetProperty("color").GetProperty("components").GetProperty("green").GetString());
			Assert.Equal("0", darkColor.GetProperty("color").GetProperty("components").GetProperty("blue").GetString());
		}

		[Fact]
		public void RasterWithoutBaseSizeGeneratesAllReferencedImageFiles()
		{
			var splash = new TaskItem("images/camera.png", new Dictionary<string, string>
			{
				["DarkFile"] = "images/camera_color.png",
			});

			var task = GetNewTask(splash);
			var success = task.Execute();
			Assert.True(success, LogErrorEvents.FirstOrDefault()?.Message);

			AssertAllImageSetFilesExist();
		}

		[Fact]
		public void DarkFileWithoutColorDoesNotGenerateColorAsset()
		{
			var splash = new TaskItem("images/camera.png", new Dictionary<string, string>
			{
				["DarkFile"] = "images/camera_color.png",
			});

			var task = GetNewTask(splash);
			var success = task.Execute();
			Assert.True(success, LogErrorEvents.FirstOrDefault()?.Message);

			AssertFileExists("Assets.xcassets/MauiSplashImage.imageset/Contents.json");
			AssertFileNotExists("Assets.xcassets/MauiSplashColor.colorset/Contents.json");
		}

		[Fact]
		public void RemovingColorMetadataDeletesStaleColorAsset()
		{
			var splashWithColor = new TaskItem("images/camera.png", new Dictionary<string, string>
			{
				["Color"] = "#ffffff",
				["DarkColor"] = "#000000",
				["DarkFile"] = "images/camera_color.png",
				["BaseSize"] = "44",
			});

			var firstTask = GetNewTask(splashWithColor);
			var firstSuccess = firstTask.Execute();
			Assert.True(firstSuccess, LogErrorEvents.FirstOrDefault()?.Message);
			AssertFileExists("Assets.xcassets/MauiSplashColor.colorset/Contents.json");

			var splashWithoutColor = new TaskItem("images/camera.png", new Dictionary<string, string>
			{
				["DarkFile"] = "images/camera_color.png",
				["BaseSize"] = "44",
			});

			var secondTask = GetNewTask(splashWithoutColor);
			var secondSuccess = secondTask.Execute();
			Assert.True(secondSuccess, LogErrorEvents.FirstOrDefault()?.Message);

			AssertFileNotExists("Assets.xcassets/MauiSplashColor.colorset/Contents.json");
		}

		[Fact]
		public void DarkTintColorOnlyGeneratesTintedDarkImage()
		{
			var splash = new TaskItem("images/camera.svg", new Dictionary<string, string>
			{
				["DarkTintColor"] = "#ff0000",
				["BaseSize"] = "44",
			});

			var task = GetNewTask(splash);
			var success = task.Execute();
			Assert.True(success, LogErrorEvents.FirstOrDefault()?.Message);

			AssertFileExists("Assets.xcassets/MauiSplashImage.imageset/Contents.json");
			AssertFileSize("Assets.xcassets/MauiSplashImage.imageset/MauiSplashImage.png", 44, 44);
			AssertFileSize("Assets.xcassets/MauiSplashImage.imageset/MauiSplashImageDark.png", 44, 44);
			AssertFileContains("Assets.xcassets/MauiSplashImage.imageset/MauiSplashImageDark.png", SKColors.Red);
			AssertFileDoesNotContain("Assets.xcassets/MauiSplashImage.imageset/MauiSplashImage.png", SKColors.Red);
			AssertFileNotExists("Assets.xcassets/MauiSplashColor.colorset/Contents.json");
		}

		[Fact]
		public void DarkColorOnlyGeneratesImageSetAndColorAssetWithWarning()
		{
			var splash = new TaskItem("images/camera.png", new Dictionary<string, string>
			{
				["DarkColor"] = "#000000",
				["BaseSize"] = "44",
			});

			var task = GetNewTask(splash);
			var success = task.Execute();
			Assert.True(success, LogErrorEvents.FirstOrDefault()?.Message);

			AssertFileExists("Assets.xcassets/MauiSplashImage.imageset/Contents.json");
			AssertFileExists("Assets.xcassets/MauiSplashColor.colorset/Contents.json");
			AssertFileSize("Assets.xcassets/MauiSplashImage.imageset/MauiSplashImage.png", 44, 44);
			AssertFileSize("Assets.xcassets/MauiSplashImage.imageset/MauiSplashImageDark.png", 44, 44);
			Assert.Contains(LogWarningEvents, warning => warning.Message.Contains("DarkColor was specified without Color", StringComparison.Ordinal));

			using var colorJson = JsonDocument.Parse(File.ReadAllText(Path.Combine(DestinationDirectory, "Assets.xcassets", "MauiSplashColor.colorset", "Contents.json")));
			var colors = colorJson.RootElement.GetProperty("colors").EnumerateArray().ToArray();
			var anyColor = Assert.Single(colors.Where(color => !color.TryGetProperty("appearances", out _)));
			var darkColor = Assert.Single(colors.Where(color => GetAppearanceValue(color) == "dark"));
			Assert.Equal("1", anyColor.GetProperty("color").GetProperty("components").GetProperty("red").GetString());
			Assert.Equal("0", darkColor.GetProperty("color").GetProperty("components").GetProperty("red").GetString());
		}

		[Fact]
		public void LaunchScreenPlistUsesNamedAssets()
		{
			var task = new CreatePartialInfoPlistTask
			{
				IntermediateOutputPath = DestinationDirectory,
				PlistName = "MauiInfo.plist",
				LaunchScreenImage = "MauiSplashImage",
				LaunchScreenColor = "MauiSplashColor",
				BuildEngine = this,
			};

			var success = task.Execute();
			Assert.True(success, LogErrorEvents.FirstOrDefault()?.Message);

			var plist = XElement.Load(Path.Combine(DestinationDirectory, "MauiInfo.plist"));
			var text = plist.ToString();
			Assert.Contains("UILaunchScreen", text, StringComparison.Ordinal);
			Assert.Contains("UIImageName", text, StringComparison.Ordinal);
			Assert.Contains("MauiSplashImage", text, StringComparison.Ordinal);
			Assert.Contains("UIColorName", text, StringComparison.Ordinal);
			Assert.Contains("MauiSplashColor", text, StringComparison.Ordinal);
			Assert.DoesNotContain("UILaunchStoryboardName", text, StringComparison.Ordinal);
		}

		[Fact]
		public void LaunchScreenPlistOmitsColorWhenNoNamedColorIsProvided()
		{
			var task = new CreatePartialInfoPlistTask
			{
				IntermediateOutputPath = DestinationDirectory,
				PlistName = "MauiInfo.plist",
				LaunchScreenImage = "MauiSplashImage",
				BuildEngine = this,
			};

			var success = task.Execute();
			Assert.True(success, LogErrorEvents.FirstOrDefault()?.Message);

			var plist = XElement.Load(Path.Combine(DestinationDirectory, "MauiInfo.plist"));
			var text = plist.ToString();
			Assert.Contains("UILaunchScreen", text, StringComparison.Ordinal);
			Assert.Contains("UIImageName", text, StringComparison.Ordinal);
			Assert.DoesNotContain("UIColorName", text, StringComparison.Ordinal);
			Assert.DoesNotContain("MauiSplashColor", text, StringComparison.Ordinal);
		}

		void AssertAllImageSetFilesExist()
		{
			var imageSetPath = Path.Combine(DestinationDirectory, "Assets.xcassets", "MauiSplashImage.imageset");
			using var imageJson = JsonDocument.Parse(File.ReadAllText(Path.Combine(imageSetPath, "Contents.json")));

			foreach (var image in imageJson.RootElement.GetProperty("images").EnumerateArray())
			{
				if (image.TryGetProperty("filename", out var filename))
					Assert.True(File.Exists(Path.Combine(imageSetPath, filename.GetString()!)), $"Expected {filename.GetString()} to exist.");
			}
		}

		static string GetAppearanceValue(JsonElement element) =>
			element.TryGetProperty("appearances", out var appearances) && appearances.GetArrayLength() > 0
				? appearances[0].GetProperty("value").GetString()
				: null;
	}
}
