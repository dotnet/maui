using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml.Linq;
using Microsoft.Build.Framework;
using Microsoft.Build.Utilities;
using Xunit;
using Xunit.Abstractions;

namespace Microsoft.Maui.Resizetizer.Tests
{
	public class GenerateSplashAndroidResourcesTests : MSBuildTaskTestFixture<GenerateSplashAndroidResources>
	{
		readonly string _colors;
		readonly string _drawable;
		readonly string _drawable_v31;

		static readonly Dictionary<string, string> ResizeMetadata = new() { ["Resize"] = "true" };

		public GenerateSplashAndroidResourcesTests(ITestOutputHelper outputHelper)
			: base(outputHelper)
		{
			_colors = Path.Combine(DestinationDirectory, "values", "maui_colors.xml");
			_drawable = Path.Combine(DestinationDirectory, "drawable", "maui_splash_image.xml");
			_drawable_v31 = Path.Combine(DestinationDirectory, "drawable-v31", "maui_splash_image.xml");
		}

		protected GenerateSplashAndroidResources GetNewTask(params ITaskItem[] splash) =>
			new()
			{
				MauiSplashScreen = splash,
				IntermediateOutputPath = DestinationDirectory,
				InputsFile = "mauisplash.inputs",
				BuildEngine = this,
			};

		[Theory]
		[InlineData("#abcdef", "#ffabcdef")]
		[InlineData("Red", "#ffff0000")]
		public void XmlIsValid(string inputColor, string outputColor)
		{
			var splash = new TaskItem("images/appiconfg.svg", new Dictionary<string, string>
			{
				["Color"] = inputColor,
			});

			var task = GetNewTask(splash);
			var success = task.Execute();
			Assert.True(success, LogErrorEvents.FirstOrDefault()?.Message);

			AssertColorsFile("maui_colors.xml", outputColor);
			AssertImageFile("maui_splash_image.xml", _drawable, "@drawable/appiconfg");
			AssertImageFile("maui_splash_image_v31.xml", _drawable_v31, "@drawable/appiconfg");
		}

		[Theory]
		[InlineData("tall_image.png", "20", "108")]
		[InlineData("wide_image.png", "108", "20")]
		public void XmlIsValidForNonSquare(string image, string width, string height)
		{
			var splash = new TaskItem("images/" + image, new Dictionary<string, string>
			{
				["Color"] = "Red",
				["Link"] = "splash_image_drawable",
			});

			var task = GetNewTask(splash);
			var success = task.Execute();
			Assert.True(success, LogErrorEvents.FirstOrDefault()?.Message);

			AssertImageFile("maui_splash_image.xml", _drawable, "@drawable/splash_image_drawable", width, height);
			AssertImageFile("maui_splash_image_v31.xml", _drawable_v31, "@drawable/splash_image_drawable", width, height);
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

			AssertImageFile("maui_splash_image.xml", _drawable, $"@drawable/{outputImage}");
			AssertImageFile("maui_splash_image_v31.xml", _drawable_v31, $"@drawable/{outputImage}");
		}

		[Fact]
		public void NoItemsSucceed()
		{
			var task = GetNewTask();

			var success = task.Execute();

			Assert.True(success, LogErrorEvents.FirstOrDefault()?.Message);
		}

		[Fact]
		public void NullItemsSucceed()
		{
			var task = GetNewTask(null);

			var success = task.Execute();

			Assert.True(success, LogErrorEvents.FirstOrDefault()?.Message);
		}

		[Fact]
		public void NonExistantFileFails()
		{
			var items = new[]
			{
				new TaskItem("non-existant.png"),
			};

			var task = GetNewTask(items);

			var success = task.Execute();

			Assert.False(success);
		}

		[Fact]
		public void AndroidResourceProcessingErrorCode()
		{
			var items = new[]
			{
				new TaskItem("non-existant.png"),
			};

			var task = GetNewTask(items);

			var success = task.Execute();

			Assert.False(success);

			var errorCode = LogErrorEvents.FirstOrDefault()?.Code;

			Assert.Equal("MAUIR0004", errorCode);
		}

		[Fact]
		public void ValidFileSucceeds()
		{
			var items = new[]
			{
				new TaskItem("images/camera.png"),
			};

			var task = GetNewTask(items);

			var success = task.Execute();

			Assert.True(success, LogErrorEvents.FirstOrDefault()?.Message);
		}

		[Fact]
		public void ValidVectorFileSucceeds()
		{
			var items = new[]
			{
				new TaskItem("images/camera.svg"),
			};

			var task = GetNewTask(items);

			var success = task.Execute();

			Assert.True(success, LogErrorEvents.FirstOrDefault()?.Message);
		}

		[Fact]
		public void SingleImageWithOnlyPathSucceeds()
		{
			var items = new[]
			{
				new TaskItem("images/camera.png", ResizeMetadata),
			};

			var task = GetNewTask(items);
			var success = task.Execute();
			Assert.True(success, LogErrorEvents.FirstOrDefault()?.Message);

			AssertFileSize("drawable-mdpi/camera.png", 1792, 1792);  // 1x
			AssertFileSize("drawable-xhdpi/camera.png", 3584, 3584); // 2x
		}

		[Fact]
		public void SingleVectorImageWithOnlyPathSucceeds()
		{
			var items = new[]
			{
				new TaskItem("images/camera.svg"),
			};

			var task = GetNewTask(items);
			var success = task.Execute();
			Assert.True(success, LogErrorEvents.FirstOrDefault()?.Message);

			AssertFileSize("drawable-mdpi/camera.png", 1792, 1792);  // 1x
			AssertFileSize("drawable-xhdpi/camera.png", 3584, 3584); // 2x
		}

		[Fact]
		public void TwoImagesWithOnlyPathOnlyGeneratesFirstImage()
		{
			var items = new[]
			{
				new TaskItem("images/camera.png", ResizeMetadata),
				new TaskItem("images/camera_color.png", ResizeMetadata),
			};

			var task = GetNewTask(items);
			var success = task.Execute();
			Assert.True(success, LogErrorEvents.FirstOrDefault()?.Message);

			AssertFileSize("drawable-mdpi/camera.png", 1792, 1792);
			AssertFileNotExists("drawable-mdpi/camera_color.png");

			AssertFileSize("drawable-xhdpi/camera.png", 3584, 3584);
			AssertFileNotExists("drawable-xhdpi/camera_color.png");
		}

		[Fact]
		public void SingleImageNoResizeSucceeds()
		{
			var items = new[]
			{
				new TaskItem("images/camera.png", new Dictionary<string, string>
				{
					["Resize"] = bool.FalseString,
				}),
			};

			var task = GetNewTask(items);
			var success = task.Execute();
			Assert.True(success, LogErrorEvents.FirstOrDefault()?.Message);

			AssertFileSize("drawable/camera.png", 1792, 1792);
			AssertFileMatches("drawable/camera.png");
		}

		[Fact]
		public void SingleVectorImageNoResizeSucceeds()
		{
			var items = new[]
			{
				new TaskItem("images/camera.svg", new Dictionary<string, string>
				{
					["Resize"] = bool.FalseString,
				}),
			};

			var task = GetNewTask(items);
			var success = task.Execute();
			Assert.True(success, LogErrorEvents.FirstOrDefault()?.Message);

			AssertFileSize("drawable/camera.png", 1792, 1792);
		}

		[Theory]
		[InlineData(null, "camera")]
		[InlineData("", "camera")]
		[InlineData("camera", "camera")]
		[InlineData("camera.png", "camera")]
		[InlineData("folder/camera.png", "camera")]
		[InlineData("the_alias", "the_alias")]
		[InlineData("the_alias.png", "the_alias")]
		[InlineData("folder/the_alias.png", "the_alias")]
		public void SingleImageWithBaseSizeSucceeds(string alias, string outputName)
		{
			var items = new[]
			{
				new TaskItem("images/camera.png", new Dictionary<string, string>
				{
					["BaseSize"] = "44",
					["Link"] = alias,
				}),
			};

			var task = GetNewTask(items);
			var success = task.Execute();
			Assert.True(success, LogErrorEvents.FirstOrDefault()?.Message);

			AssertFileSize($"drawable-mdpi/{outputName}.png", 44, 44);
			AssertFileSize($"drawable-xhdpi/{outputName}.png", 88, 88);
		}

		void AssertColorsFile(string expectedFilename, string color)
		{
			var expectedXml = File.ReadAllText($"testdata/androidsplash/" + expectedFilename)
				.Replace("{maui_splash_color}", color, StringComparison.OrdinalIgnoreCase);

			var actual = XElement.Load(_colors);
			var expected = XElement.Parse(expectedXml);

			Assert.True(XNode.DeepEquals(actual, expected), $"{_colors} did not match:\n{actual}");
		}

		void AssertImageFile(string expectedFilename, string actualFilename, string image, string width = "108", string height = "108")
		{
			var expectedXml = File.ReadAllText($"testdata/androidsplash/" + expectedFilename)
				.Replace("{drawable}", image, StringComparison.OrdinalIgnoreCase)
				.Replace("{width}", width, StringComparison.OrdinalIgnoreCase)
				.Replace("{height}", height, StringComparison.OrdinalIgnoreCase);

			var actual = XElement.Load(actualFilename);
			var expected = XElement.Parse(expectedXml);

			Assert.True(XNode.DeepEquals(actual, expected), $"{actualFilename} did not match:\n{actual}");
		}
	}
}
