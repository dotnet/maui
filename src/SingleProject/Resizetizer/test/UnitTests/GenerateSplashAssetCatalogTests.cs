using System;
using System.Collections.Generic;
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
			Assert.All(colors, color =>
				Assert.Equal("1.0", color.GetProperty("color").GetProperty("components").GetProperty("alpha").GetString()));
			var darkColor = Assert.Single(colors, color => GetAppearanceValue(color) == "dark");
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
		public void RasterWithoutResizePreservesOriginalImageDimensions()
		{
			var splash = new TaskItem("images/camera.png", new Dictionary<string, string>
			{
				["Resize"] = bool.FalseString,
				["DarkFile"] = "images/camera_color.png",
			});

			var task = GetNewTask(splash);
			var success = task.Execute();
			Assert.True(success, LogErrorEvents.FirstOrDefault()?.Message);

			AssertFileSize("Assets.xcassets/MauiSplashImage.imageset/MauiSplashImage.png", 1792, 1792);
			AssertFileSize("Assets.xcassets/MauiSplashImage.imageset/MauiSplashImage@2x.png", 1792, 1792);
			AssertFileSize("Assets.xcassets/MauiSplashImage.imageset/MauiSplashImage@3x.png", 1792, 1792);
			AssertFileSize("Assets.xcassets/MauiSplashImage.imageset/MauiSplashImageDark.png", 256, 256);
			AssertFileSize("Assets.xcassets/MauiSplashImage.imageset/MauiSplashImageDark@2x.png", 256, 256);
			AssertFileSize("Assets.xcassets/MauiSplashImage.imageset/MauiSplashImageDark@3x.png", 256, 256);
		}

		[Theory]
		[InlineData("MauiSplashImage.png", "light")]
		[InlineData("MauiSplashImageDark.png", "dark")]
		public void ResizeQualityMetadataAffectsImageAsset(string filename, string appearance)
		{
			var fastestSplash = new TaskItem("images/camera.png", new Dictionary<string, string>
			{
				["BaseSize"] = "64",
				["DarkFile"] = "images/camera.png",
				["ResizeQuality"] = "Fastest",
			});

			var task = GetNewTask(fastestSplash);
			var success = task.Execute();
			Assert.True(success, LogErrorEvents.FirstOrDefault()?.Message);
			var output = $"Assets.xcassets/MauiSplashImage.imageset/{filename}";
			AssertFileSize(output, 64, 64);
			var fastestPixels = ReadPixels(output);

			var autoSplash = new TaskItem("images/camera.png", new Dictionary<string, string>
			{
				["BaseSize"] = "64",
				["DarkFile"] = "images/camera.png",
				["ResizeQuality"] = "Auto",
			});

			task = GetNewTask(autoSplash);
			success = task.Execute();
			Assert.True(success, LogErrorEvents.FirstOrDefault()?.Message);
			AssertFileSize(output, 64, 64);

			var autoPixels = ReadPixels(output);
			var differentPixels = AssertPixelsDiffer(fastestPixels, autoPixels,
				$"Apple {appearance} splash output should honor ResizeQuality metadata during 1792-to-64 downscaling.");
			Output.WriteLine($"Apple {appearance} Fastest vs Auto: {differentPixels} of {autoPixels.Length} pixels differ.");
		}

		[Fact]
		public void NonPngRasterWithoutResizeUsesMatchingAssetFilenames()
		{
			var inputDirectory = Path.Combine(Path.GetTempPath(), "Microsoft.Maui.Resizetizer.Tests", nameof(GenerateSplashAssetCatalogTests), Path.GetRandomFileName());
			var lightFile = CopyImageWithExtension(inputDirectory, "camera.jpg", "images/camera.png");
			var darkFile = CopyImageWithExtension(inputDirectory, "camera_color.jpg", "images/camera_color.png");
			var splash = new TaskItem(lightFile, new Dictionary<string, string>
			{
				["Resize"] = bool.FalseString,
				["DarkFile"] = darkFile,
			});
			var imageSetPath = Path.Combine(DestinationDirectory, "Assets.xcassets", "MauiSplashImage.imageset");
			Directory.CreateDirectory(imageSetPath);
			File.WriteAllText(Path.Combine(imageSetPath, "MauiSplashImage.png"), "stale");
			File.WriteAllText(Path.Combine(imageSetPath, "MauiSplashImageDark.png"), "stale");

			var task = GetNewTask(splash);
			var success = task.Execute();
			Assert.True(success, LogErrorEvents.FirstOrDefault()?.Message);

			AssertFileExists("Assets.xcassets/MauiSplashImage.imageset/MauiSplashImage.jpg");
			AssertFileExists("Assets.xcassets/MauiSplashImage.imageset/MauiSplashImage@2x.jpg");
			AssertFileExists("Assets.xcassets/MauiSplashImage.imageset/MauiSplashImageDark.jpg");
			AssertFileExists("Assets.xcassets/MauiSplashImage.imageset/MauiSplashImageDark@2x.jpg");
			AssertFileNotExists("Assets.xcassets/MauiSplashImage.imageset/MauiSplashImage.png");
			AssertFileNotExists("Assets.xcassets/MauiSplashImage.imageset/MauiSplashImageDark.png");

			using var imageJson = JsonDocument.Parse(File.ReadAllText(Path.Combine(DestinationDirectory, "Assets.xcassets", "MauiSplashImage.imageset", "Contents.json")));
			var filenames = imageJson.RootElement.GetProperty("images").EnumerateArray()
				.Select(image => image.GetProperty("filename").GetString())
				.ToArray();
			Assert.Contains("MauiSplashImage.jpg", filenames);
			Assert.Contains("MauiSplashImage@2x.jpg", filenames);
			Assert.Contains("MauiSplashImageDark.jpg", filenames);
			Assert.Contains("MauiSplashImageDark@2x.jpg", filenames);
			Assert.DoesNotContain("MauiSplashImage.png", filenames);
			Assert.DoesNotContain("MauiSplashImageDark.png", filenames);
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
			var anyColor = Assert.Single(colors, color => !color.TryGetProperty("appearances", out _));
			var darkColor = Assert.Single(colors, color => GetAppearanceValue(color) == "dark");
			Assert.Equal("1.0", anyColor.GetProperty("color").GetProperty("components").GetProperty("red").GetString());
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

		static string CopyImageWithExtension(string inputDirectory, string filename, string sourceFile)
		{
			Directory.CreateDirectory(inputDirectory);
			var destination = Path.Combine(inputDirectory, filename);
			File.Copy(sourceFile, destination);

			return destination;
		}
	}
}
