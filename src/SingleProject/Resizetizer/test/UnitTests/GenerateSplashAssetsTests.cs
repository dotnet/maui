using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Microsoft.Build.Framework;
using Microsoft.Build.Utilities;
using SkiaSharp;
using Xunit;
using Xunit.Abstractions;

namespace Microsoft.Maui.Resizetizer.Tests
{
	public class GenerateSplashAssetsTests : MSBuildTaskTestFixture<GenerateSplashAssets>
	{
		public GenerateSplashAssetsTests(ITestOutputHelper output)
			: base(output)
		{
		}

		protected GenerateSplashAssets GetNewTask(ITaskItem splash) =>
			new()
			{
				IntermediateOutputPath = DestinationDirectory,
				MauiSplashScreen = new[] { splash },
				BuildEngine = this,
			};

		void AssertFile(string file, int width, int height)
		{
			file = Path.Combine(DestinationDirectory, file);

			Assert.True(File.Exists(file), $"File did not exist: {file}");

			using var codec = SKCodec.Create(file);
			Assert.Equal(width, codec.Info.Width);
			Assert.Equal(height, codec.Info.Height);
		}

		[Theory]
		[InlineData("dotnet_logo", "#512BD4")]
		[InlineData("appiconfg", "#0000FF")]
		public void FileIsGenerated(string image, string color)
		{
			var splash = new TaskItem($"images/{image}.svg", new Dictionary<string, string>
			{
				["Color"] = color,
			});

			var task = GetNewTask(splash);
			var success = task.Execute();
			Assert.True(success, LogErrorEvents.FirstOrDefault()?.Message);

			AssertFile($"{image}SplashScreen.scale-100.png", 620, 300);
			AssertFile($"{image}SplashScreen.scale-125.png", 775, 375);
			AssertFile($"{image}SplashScreen.scale-200.png", 1240, 600);
		}

		[Fact]
		public void ResizeQualityMetadataIsRespected()
		{
			var fastestSplash = new TaskItem("images/camera.png", new Dictionary<string, string>
			{
				["Link"] = "camera_fastest",
				["ResizeQuality"] = "Fastest",
			});

			var task = GetNewTask(fastestSplash);
			var success = task.Execute();
			Assert.True(success, LogErrorEvents.FirstOrDefault()?.Message);

			var fastestPixels = ReadPixels("camera_fastestSplashScreen.scale-100.png");

			var autoSplash = new TaskItem("images/camera.png", new Dictionary<string, string>
			{
				["Link"] = "camera_auto",
				["ResizeQuality"] = "Auto",
			});

			task = GetNewTask(autoSplash);
			success = task.Execute();
			Assert.True(success, LogErrorEvents.FirstOrDefault()?.Message);

			AssertPixelsDiffer(fastestPixels, ReadPixels("camera_autoSplashScreen.scale-100.png"),
				"Splash output should honor ResizeQuality metadata.");
		}

		[Fact]
		public void SplashInputsIncludeResizeQualityMetadata()
		{
			var targetsFile = Path.GetFullPath(Path.Combine(GetTestProjectRoot(), "../../src/nuget/buildTransitive/Microsoft.Maui.Resizetizer.After.targets"));
			var targets = File.ReadAllText(targetsFile);

			Assert.Contains("FileHash=%(FileHash);ResizeQuality=%(ResizeQuality)", targets, StringComparison.Ordinal);
		}

		[Theory]
		[InlineData(null, "appiconfg")]
		[InlineData("images/CustomAlias.svg", "CustomAlias")]
		public void SplashScreenResectsAlias(string alias, string outputImage)
		{
			var splash = new TaskItem("images/appiconfg.svg", new Dictionary<string, string>
			{
				["Link"] = alias,
			});

			var task = GetNewTask(splash);
			var success = task.Execute();
			Assert.True(success, LogErrorEvents.FirstOrDefault()?.Message);

			AssertFile($"{outputImage}SplashScreen.scale-100.png", 620, 300);
			AssertFile($"{outputImage}SplashScreen.scale-125.png", 775, 375);
			AssertFile($"{outputImage}SplashScreen.scale-200.png", 1240, 600);
		}
	}
}
