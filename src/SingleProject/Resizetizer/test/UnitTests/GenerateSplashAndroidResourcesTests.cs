using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml.Linq;
using Microsoft.Build.Framework;
using Microsoft.Build.Utilities;
using Xunit;

namespace Microsoft.Maui.Resizetizer.Tests
{
	public class GenerateSplashAndroidResourcesTests : MSBuildTaskTestFixture<GenerateSplashAndroidResources>
	{
		readonly string _colors;
		readonly string _drawable;
		readonly string _drawable_v31;

		public GenerateSplashAndroidResourcesTests()
		{
			_colors = Path.Combine(DestinationDirectory, "values", "maui_colors.xml");
			_drawable = Path.Combine(DestinationDirectory, "drawable", "maui_splash_image.xml");
			_drawable_v31 = Path.Combine(DestinationDirectory, "drawable-v31", "maui_splash_image.xml");
		}

		protected GenerateSplashAndroidResources GetNewTask(ITaskItem splash) =>
			new()
			{
				MauiSplashScreen = new[] { splash },
				IntermediateOutputPath = DestinationDirectory,
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
