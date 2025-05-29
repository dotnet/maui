using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml.Linq;
using Microsoft.Build.Framework;
using Microsoft.Build.Utilities;
using SkiaSharp;
using Xunit;
using Xunit.Abstractions;

namespace Microsoft.Maui.Resizetizer.Tests
{
	public class GenerateSplashStoryboardTests : MSBuildTaskTestFixture<GenerateSplashStoryboard>
	{
		static readonly Dictionary<string, string> ResizeMetadata = new() { ["Resize"] = "true" };

		readonly string _storyboard;

		public GenerateSplashStoryboardTests(ITestOutputHelper outputHelper)
			: base(outputHelper)
		{
			_storyboard = Path.Combine(DestinationDirectory, "MauiSplash.storyboard");
		}

		protected GenerateSplashStoryboard GetNewTask(params ITaskItem[] splash) =>
			new()
			{
				IntermediateOutputPath = DestinationDirectory,
				InputsFile = "mauisplash.inputs",
				MauiSplashScreen = splash,
				BuildEngine = this,
			};

		void AssertFile(string actualPath, string image, string r, string g, string b, string a)
		{
			using var actualStream = File.OpenRead(actualPath);
			var actual = XElement.Load(actualStream);

			using var expectedBuilder = new StringWriter();
			GenerateSplashStoryboard.SubstituteStoryboard(expectedBuilder, "MauiSplash.storyboard", image, r, g, b, a);
			var expected = XElement.Parse(expectedBuilder.ToString());

			Assert.True(XNode.DeepEquals(actual, expected), $"{actualPath} did not match:\n{actual}");
		}

		void AssertFile(string actualPath)
		{
			using var actualStream = File.OpenRead(actualPath);
			var actual = XElement.Load(actualStream);

			using var expectedBuilder = new StringWriter();
			GenerateSplashStoryboard.SubstituteStoryboard(expectedBuilder, "MauiNoSplash.storyboard", null, SKColors.White);
			var expected = XElement.Parse(expectedBuilder.ToString());

			Assert.True(XNode.DeepEquals(actual, expected), $"{actualPath} did not match:\n{actual}");
		}

		[Theory]
		[InlineData("#abcdef", "0.67058825", "0.8039216", "0.9372549", "1")]
		[InlineData("Green", "0", "0.5019608", "0", "1")]
		public void XmlIsValid(string inputColor, string r, string g, string b, string a)
		{
			var splash = new TaskItem("images/appiconfg.svg", new Dictionary<string, string>
			{
				["Color"] = inputColor,
			});

			var task = GetNewTask(splash);
			var success = task.Execute();
			Assert.True(success, LogErrorEvents.FirstOrDefault()?.Message);

			AssertFile(_storyboard, "appiconfg.png", r, g, b, a);
		}

		[Fact]
		public void XmlIsValidForNoSplash()
		{
			var task = GetNewTask();
			var success = task.Execute();
			Assert.True(success, LogErrorEvents.FirstOrDefault()?.Message);

			AssertFile(_storyboard);
		}

		[Theory]
		[InlineData(null, "appiconfg.png")]
		[InlineData("images/CustomAlias.svg", "CustomAlias.png")]
		public void SplashScreenResectsAlias(string alias, string outputImage)
		{
			var splash = new TaskItem("images/appiconfg.svg", new Dictionary<string, string>
			{
				["Link"] = alias,
			});

			var task = GetNewTask(splash);
			var success = task.Execute();
			Assert.True(success, LogErrorEvents.FirstOrDefault()?.Message);

			AssertFile(_storyboard, outputImage, "1", "1", "1", "1");
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
		public void ApppleResourceProcessingErrorCode()
		{
			var items = new[]
			{
				new TaskItem("non-existant.png"),
			};

			var task = GetNewTask(items);

			var success = task.Execute();

			Assert.False(success);

			var errorCode = LogErrorEvents.FirstOrDefault()?.Code;

			Assert.Equal("MAUIR0005", errorCode);
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
		public void SingleImageWithOnlyPathSucceeds()
		{
			var items = new[]
			{
				new TaskItem("images/camera.png", ResizeMetadata),
			};

			var task = GetNewTask(items);
			var success = task.Execute();
			Assert.True(success, LogErrorEvents.FirstOrDefault()?.Message);

			AssertFileSize("camera.png", 1792, 1792);
			AssertFileSize("camera@2x.png", 3584, 3584);
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

			AssertFileSize("camera.png", 1792, 1792);
			AssertFileNotExists("camera_color.png");

			AssertFileSize("camera@2x.png", 3584, 3584);
			AssertFileNotExists("camera_color@2x.png");
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

			AssertFileSize($"{outputName}.png", 44, 44);
			AssertFileSize($"{outputName}@2x.png", 88, 88);
		}
	}
}
