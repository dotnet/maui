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

		public GenerateSplashAndroidResourcesTests()
		{
			_colors = Path.Combine(DestinationDirectory, "maui_colors.xml");
			_drawable = Path.Combine(DestinationDirectory, "maui_splash_image.xml");
		}

		protected GenerateSplashAndroidResources GetNewTask(ITaskItem splash) =>
			new()
			{
				MauiSplashScreen = new[] { splash },
				ColorsFile = _colors,
				DrawableFile = _drawable,
				BuildEngine = this,
			};

		void AssertColorsFile(string color)
		{
			var expectedXml = File.ReadAllText($"testdata/androidsplash/maui_colors.xml")
				.Replace("{maui_splash_color}", color, StringComparison.OrdinalIgnoreCase);

			var actual = XElement.Load(_colors);
			var expected = XElement.Parse(expectedXml);

			Assert.True(XNode.DeepEquals(actual, expected), $"{_colors} did not match:\n{actual}");
		}

		void AssertImageFile(string image)
		{
			var expectedXml = File.ReadAllText($"testdata/androidsplash/maui_splash_image.xml")
				.Replace("{drawable}", image, StringComparison.OrdinalIgnoreCase);

			var actual = XElement.Load(_drawable);
			var expected = XElement.Parse(expectedXml);

			Assert.True(XNode.DeepEquals(actual, expected), $"{_drawable} did not match:\n{actual}");
		}

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

			AssertColorsFile(outputColor);
			AssertImageFile("@drawable/appiconfg");
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

			AssertImageFile($"@drawable/{outputImage}");
		}
	}
}
