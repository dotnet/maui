using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using Microsoft.Build.Framework;
using Microsoft.Build.Utilities;
using SkiaSharp;
using Xunit;

namespace Microsoft.Maui.Resizetizer.Tests
{
	public class ResizetizeImagesTests
	{
		public abstract class ExecuteForApp : MSBuildTaskTestFixture<ResizetizeImages>
		{
			protected static readonly Dictionary<string, string> ResizeMetadata = new() { ["Resize"] = "true" };

			protected ResizetizeImages GetNewTask(string type, params ITaskItem[] items) =>
				new ResizetizeImages
				{
					DuplicateOutputErrorMessage = "Duplicate File: ",
					PlatformType = type,
					IntermediateOutputPath = DestinationDirectory,
					InputsFile = "mauiimage.inputs",
					Images = items,
					BuildEngine = this,
				};

			protected ITaskItem GetCopiedResource(ResizetizeImages task, string path) =>
				task.CopiedResources.Single(c => c.ItemSpec.Replace('\\', '/').EndsWith(path, StringComparison.Ordinal));
		}

		public abstract class ExecuteForPlatformApp : ExecuteForApp
		{
			protected abstract string Platform { get; }

			protected abstract string GetPlatformOutputFileName(string file);

			protected ResizetizeImages GetNewTask(params ITaskItem[] items) =>
				GetNewTask(Platform, items);

			[Fact]
			public void GenerationSkippedOnIncrementalBuild()
			{
				var items = new[]
				{
					new TaskItem("images/dotnet_logo.svg", new Dictionary<string, string>
					{
						["IsAppIcon"] = bool.TrueString,
						["ForegroundFile"] = $"images/dotnet_foreground.svg",
						["Link"] = "appicon",
						["BackgroundFile"] = $"images/dotnet_background.svg",
					}),
				};

				var task = GetNewTask(items);
				var success = task.Execute();
				Assert.True(success, LogErrorEvents.FirstOrDefault()?.Message);

				LogErrorEvents.Clear();
				LogMessageEvents.Clear();
				task = GetNewTask(items);
				success = task.Execute();
				Assert.True(success, LogErrorEvents.FirstOrDefault()?.Message);

				Assert.True(LogMessageEvents.Any(x => x.Message.Contains("Skipping ", StringComparison.OrdinalIgnoreCase)), $"Image generation should have been skipped.");
			}

			[Fact]
			public void CanUseSameFileMultipleTimesWithDifferentLinks()
			{
				var items = new[]
				{
					new TaskItem("images/camera.svg", new Dictionary<string, string>
					{
						["Link"] = "ImageOne",
					}),
					new TaskItem("images/camera.svg", new Dictionary<string, string>
					{
						["Link"] = "ImageTwo",
					}),
				};

				var task = GetNewTask(items);
				var success = task.Execute();
				Assert.True(success, LogErrorEvents.FirstOrDefault()?.Message);

				AssertFileExists(GetPlatformOutputFileName("ImageOne.png"));
				AssertFileSize(GetPlatformOutputFileName("ImageOne.png"), 1792, 1792);

				AssertFileExists(GetPlatformOutputFileName("ImageTwo.png"));
				AssertFileSize(GetPlatformOutputFileName("ImageTwo.png"), 1792, 1792);
			}

			[Fact]
			public void CanUseSameFileMultipleTimesWhenOneHasALink()
			{
				var items = new[]
				{
					new TaskItem("images/camera.svg"),
					new TaskItem("images/camera.svg", new Dictionary<string, string>
					{
						["Link"] = "ImageOne",
					}),
				};

				var task = GetNewTask(items);
				var success = task.Execute();
				Assert.True(success, LogErrorEvents.FirstOrDefault()?.Message);

				AssertFileExists(GetPlatformOutputFileName("camera.png"));
				AssertFileSize(GetPlatformOutputFileName("camera.png"), 1792, 1792);

				AssertFileExists(GetPlatformOutputFileName("ImageOne.png"));
				AssertFileSize(GetPlatformOutputFileName("ImageOne.png"), 1792, 1792);
			}

			[Fact]
			public void FailsOnExactMatchingMultipleFiles()
			{
				var items = new[]
				{
					new TaskItem("images/camera.svg", new Dictionary<string, string>
					{
						["Link"] = "ImageOne",
					}),
					new TaskItem("images/camera.svg", new Dictionary<string, string>
					{
						["Link"] = "ImageOne",
					}),
				};

				var task = GetNewTask(items);
				var success = task.Execute();
				Assert.False(success, "Expected an error about duplicates");

				Assert.True(LogErrorEvents.Any(x => x.Message.StartsWith("Duplicate File: ", StringComparison.OrdinalIgnoreCase)), LogErrorEvents.FirstOrDefault()?.Message);
			}

			[Fact]
			public void FailsOnExactMatchingMultipleFilesWhenTheMetadataIsNotRelevant()
			{
				var items = new[]
				{
					new TaskItem("images/camera.svg"),
					new TaskItem("images/camera.svg", new Dictionary<string, string>
					{
						["SomeRandomMetadata"] = "NotMe",
					}),
				};

				var task = GetNewTask(items);
				var success = task.Execute();
				Assert.False(success, "Expected an error about duplicates");

				Assert.True(LogErrorEvents.Any(x => x.Message.StartsWith("Duplicate File: ", StringComparison.OrdinalIgnoreCase)), LogErrorEvents.FirstOrDefault()?.Message);
			}

			[Fact]
			public void FailsOnAlmostExactMatchingMultipleFiles()
			{
				var items = new[]
				{
					new TaskItem("images/camera.svg"),
					new TaskItem("images/camera.svg", new Dictionary<string, string>
					{
						["TintColor"] = "#FF00FF",
					}),
				};

				var task = GetNewTask(items);
				var success = task.Execute();
				Assert.False(success, "Expected an error about duplicates");

				Assert.True(LogErrorEvents.Any(x => x.Message.StartsWith("Duplicate File: ", StringComparison.OrdinalIgnoreCase)), LogErrorEvents.FirstOrDefault()?.Message);
			}
		}

		public class ExecuteForAndroid : ExecuteForPlatformApp
		{
			protected override string Platform => "android";

			protected override string GetPlatformOutputFileName(string file) =>
				$"drawable-mdpi/{file}";

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
			public void TwoImagesWithOnlyPathSucceed()
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
				AssertFileSize("drawable-mdpi/camera_color.png", 256, 256);

				AssertFileSize("drawable-xhdpi/camera.png", 3584, 3584);
				AssertFileSize("drawable-xhdpi/camera_color.png", 512, 512);
			}

			[Fact]
			public void ImageWithOnlyPathHasMetadata()
			{
				var items = new[]
				{
					new TaskItem("images/camera.png", ResizeMetadata),
				};

				var task = GetNewTask(items);
				var success = task.Execute();
				Assert.True(success, LogErrorEvents.FirstOrDefault()?.Message);

				var copied = task.CopiedResources;
				Assert.Equal(items.Length * DpiPath.Android.Image.Length, copied.Length);

				var mdpi = GetCopiedResource(task, "drawable-mdpi/camera.png");
				Assert.Equal("drawable-mdpi", mdpi.GetMetadata("_ResizetizerDpiPath"));
				Assert.Equal("1.0", mdpi.GetMetadata("_ResizetizerDpiScale"));

				var xhdpi = GetCopiedResource(task, "drawable-xhdpi/camera.png");
				Assert.Equal("drawable-xhdpi", xhdpi.GetMetadata("_ResizetizerDpiPath"));
				Assert.Equal("2.0", xhdpi.GetMetadata("_ResizetizerDpiScale"));
			}

			[Fact]
			public void TwoImagesWithOnlyPathHasMetadata()
			{
				var items = new[]
				{
					new TaskItem("images/camera.png", ResizeMetadata),
					new TaskItem("images/camera_color.png", ResizeMetadata),
				};

				var task = GetNewTask(items);
				var success = task.Execute();
				Assert.True(success, LogErrorEvents.FirstOrDefault()?.Message);

				var copied = task.CopiedResources;
				Assert.Equal(items.Length * DpiPath.Android.Image.Length, copied.Length);

				var mdpi = GetCopiedResource(task, "drawable-mdpi/camera.png");
				Assert.Equal("drawable-mdpi", mdpi.GetMetadata("_ResizetizerDpiPath"));
				Assert.Equal("1.0", mdpi.GetMetadata("_ResizetizerDpiScale"));

				var xhdpi = GetCopiedResource(task, "drawable-xhdpi/camera.png");
				Assert.Equal("drawable-xhdpi", xhdpi.GetMetadata("_ResizetizerDpiPath"));
				Assert.Equal("2.0", xhdpi.GetMetadata("_ResizetizerDpiScale"));

				mdpi = GetCopiedResource(task, "drawable-mdpi/camera_color.png");
				Assert.Equal("drawable-mdpi", mdpi.GetMetadata("_ResizetizerDpiPath"));
				Assert.Equal("1.0", mdpi.GetMetadata("_ResizetizerDpiScale"));

				xhdpi = GetCopiedResource(task, "drawable-xhdpi/camera_color.png");
				Assert.Equal("drawable-xhdpi", xhdpi.GetMetadata("_ResizetizerDpiPath"));
				Assert.Equal("2.0", xhdpi.GetMetadata("_ResizetizerDpiScale"));
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

			[Theory]
			[InlineData("camera", null, "camera")]
			[InlineData("camera", "", "camera")]
			[InlineData("camera", "camera", "camera")]
			[InlineData("camera", "camera.png", "camera")]
			[InlineData("camera", "folder/camera.png", "camera")]
			[InlineData("camera", "the_alias", "the_alias")]
			[InlineData("camera", "the_alias.png", "the_alias")]
			[InlineData("camera", "folder/the_alias.png", "the_alias")]
			[InlineData("camera_color", null, "camera_color")]
			[InlineData("camera_color", "", "camera_color")]
			[InlineData("camera_color", "camera_color", "camera_color")]
			[InlineData("camera_color", "camera_color.png", "camera_color")]
			[InlineData("camera_color", "folder/camera_color.png", "camera_color")]
			[InlineData("camera_color", "the_alias", "the_alias")]
			[InlineData("camera_color", "the_alias.png", "the_alias")]
			[InlineData("camera_color", "folder/the_alias.png", "the_alias")]
			public void SingleRasterAppIconWithOnlyPathSucceedsWithoutVectors(string name, string alias, string outputName)
			{
				var items = new[]
				{
					new TaskItem($"images/{name}.png", new Dictionary<string, string>
					{
						["IsAppIcon"] = bool.TrueString,
						["Link"] = alias,
					}),
				};

				var task = GetNewTask(items);
				var success = task.Execute();
				Assert.True(success, LogErrorEvents.FirstOrDefault()?.Message);

				AssertFileSize($"mipmap-mdpi/{outputName}.png", 48, 48);
				AssertFileSize($"mipmap-mdpi/{outputName}_background.png", 108, 108);
				AssertFileSize($"mipmap-mdpi/{outputName}_foreground.png", 108, 108);

				AssertFileSize($"mipmap-xhdpi/{outputName}.png", 96, 96);
				AssertFileSize($"mipmap-xhdpi/{outputName}_background.png", 216, 216);
				AssertFileSize($"mipmap-xhdpi/{outputName}_foreground.png", 216, 216);

				AssertFileExists($"mipmap-anydpi-v26/{outputName}.xml");
				AssertFileExists($"mipmap-anydpi-v26/{outputName}_round.xml");

				AssertFileContains($"mipmap-anydpi-v26/{outputName}.xml",
					$"<foreground android:drawable=\"@mipmap/{outputName}_foreground\"/>",
					$"<background android:drawable=\"@mipmap/{outputName}_background\"/>");

				AssertFileContains($"mipmap-anydpi-v26/{outputName}_round.xml",
					$"<foreground android:drawable=\"@mipmap/{outputName}_foreground\"/>",
					$"<background android:drawable=\"@mipmap/{outputName}_background\"/>");

				AssertFileMatches($"mipmap-mdpi/{outputName}.png", new object[] { name, alias, "m", "i" });
				AssertFileMatches($"mipmap-mdpi/{outputName}_background.png", new object[] { name, alias, "m", "b" });
				AssertFileMatches($"mipmap-mdpi/{outputName}_foreground.png", new object[] { name, alias, "m", "f" });
				AssertFileMatches($"mipmap-xhdpi/{outputName}.png", new object[] { name, alias, "xh", "i" });
				AssertFileMatches($"mipmap-xhdpi/{outputName}_background.png", new object[] { name, alias, "xh", "b" });
				AssertFileMatches($"mipmap-xhdpi/{outputName}_foreground.png", new object[] { name, alias, "xh", "f" });
			}

			[Theory]
			[InlineData("appicon", null, "appicon")]
			[InlineData("appicon", "", "appicon")]
			[InlineData("appicon", "appicon", "appicon")]
			[InlineData("appicon", "appicon.png", "appicon")]
			[InlineData("appicon", "folder/appicon.png", "appicon")]
			[InlineData("appicon", "the_alias", "the_alias")]
			[InlineData("appicon", "the_alias.png", "the_alias")]
			[InlineData("appicon", "folder/the_alias.png", "the_alias")]
			[InlineData("camera", null, "camera")]
			[InlineData("camera", "", "camera")]
			[InlineData("camera", "camera", "camera")]
			[InlineData("camera", "camera.png", "camera")]
			[InlineData("camera", "folder/camera.png", "camera")]
			[InlineData("camera", "the_alias", "the_alias")]
			[InlineData("camera", "the_alias.png", "the_alias")]
			[InlineData("camera", "folder/the_alias.png", "the_alias")]
			public void SingleVectorAppIconWithOnlyPathSucceedsWithoutVectors(string name, string alias, string outputName)
			{
				var items = new[]
				{
					new TaskItem($"images/{name}.svg", new Dictionary<string, string>
					{
						["IsAppIcon"] = bool.TrueString,
						["Link"] = alias,
					}),
				};

				var task = GetNewTask(items);
				var success = task.Execute();
				Assert.True(success, LogErrorEvents.FirstOrDefault()?.Message);

				AssertFileSize($"mipmap-mdpi/{outputName}.png", 48, 48);
				AssertFileSize($"mipmap-mdpi/{outputName}_background.png", 108, 108);
				AssertFileSize($"mipmap-mdpi/{outputName}_foreground.png", 108, 108);

				AssertFileSize($"mipmap-xhdpi/{outputName}.png", 96, 96);
				AssertFileSize($"mipmap-xhdpi/{outputName}_background.png", 216, 216);
				AssertFileSize($"mipmap-xhdpi/{outputName}_foreground.png", 216, 216);

				AssertFileExists($"mipmap-anydpi-v26/{outputName}.xml");
				AssertFileExists($"mipmap-anydpi-v26/{outputName}_round.xml");

				AssertFileContains($"mipmap-anydpi-v26/{outputName}.xml",
					$"<foreground android:drawable=\"@mipmap/{outputName}_foreground\"/>",
					$"<background android:drawable=\"@mipmap/{outputName}_background\"/>");

				AssertFileContains($"mipmap-anydpi-v26/{outputName}_round.xml",
					$"<foreground android:drawable=\"@mipmap/{outputName}_foreground\"/>",
					$"<background android:drawable=\"@mipmap/{outputName}_background\"/>");

				AssertFileMatches($"mipmap-mdpi/{outputName}.png", new object[] { name, alias, "m", "i" });
				AssertFileMatches($"mipmap-mdpi/{outputName}_background.png", new object[] { name, alias, "m", "b" });
				AssertFileMatches($"mipmap-mdpi/{outputName}_foreground.png", new object[] { name, alias, "m", "f" });
				AssertFileMatches($"mipmap-xhdpi/{outputName}.png", new object[] { name, alias, "xh", "i" });
				AssertFileMatches($"mipmap-xhdpi/{outputName}_background.png", new object[] { name, alias, "xh", "b" });
				AssertFileMatches($"mipmap-xhdpi/{outputName}_foreground.png", new object[] { name, alias, "xh", "f" });
			}

			[Theory]
			[InlineData("appicon", null, "dotnet_background")]
			[InlineData("appicon", "", "dotnet_background")]
			[InlineData("appicon", "appicon", "appicon")]
			[InlineData("appicon", "appicon.png", "appicon")]
			[InlineData("appicon", "folder/appicon.png", "appicon")]
			[InlineData("appicon", "the_alias", "the_alias")]
			[InlineData("appicon", "the_alias.png", "the_alias")]
			[InlineData("appicon", "folder/the_alias.png", "the_alias")]
			[InlineData("camera", null, "dotnet_background")]
			[InlineData("camera", "", "dotnet_background")]
			[InlineData("camera", "camera", "camera")]
			[InlineData("camera", "camera.png", "camera")]
			[InlineData("camera", "folder/camera.png", "camera")]
			[InlineData("camera", "the_alias", "the_alias")]
			[InlineData("camera", "the_alias.png", "the_alias")]
			[InlineData("camera", "folder/the_alias.png", "the_alias")]
			[InlineData("prismicon", "rasters", "rasters")]
			public void MultipleVectorAppIconWithOnlyPathConvertsToRaster(string name, string alias, string outputName)
			{
				var items = new[]
				{
					new TaskItem("images/dotnet_background.svg", new Dictionary<string, string>
					{
						["IsAppIcon"] = bool.TrueString,
						["ForegroundFile"] = $"images/{name}.svg",
						["Link"] = alias,
					}),
				};

				var task = GetNewTask(items);
				var success = task.Execute();
				Assert.True(success, LogErrorEvents.FirstOrDefault()?.Message);

				AssertFileSize($"mipmap-mdpi/{outputName}.png", 48, 48);
				AssertFileSize($"mipmap-mdpi/{outputName}_background.png", 108, 108);
				AssertFileSize($"mipmap-mdpi/{outputName}_foreground.png", 108, 108);

				AssertFileSize($"mipmap-xhdpi/{outputName}.png", 96, 96);
				AssertFileSize($"mipmap-xhdpi/{outputName}_background.png", 216, 216);
				AssertFileSize($"mipmap-xhdpi/{outputName}_foreground.png", 216, 216);

				AssertFileExists($"mipmap-anydpi-v26/{outputName}.xml");
				AssertFileExists($"mipmap-anydpi-v26/{outputName}_round.xml");

				AssertFileContains($"mipmap-anydpi-v26/{outputName}.xml",
					$"<foreground android:drawable=\"@mipmap/{outputName}_foreground\"/>",
					$"<background android:drawable=\"@mipmap/{outputName}_background\"/>");

				AssertFileContains($"mipmap-anydpi-v26/{outputName}_round.xml",
					$"<foreground android:drawable=\"@mipmap/{outputName}_foreground\"/>",
					$"<background android:drawable=\"@mipmap/{outputName}_background\"/>");

				AssertFileMatches($"mipmap-mdpi/{outputName}.png", new object[] { name, alias, "m", "i" });
				AssertFileMatches($"mipmap-mdpi/{outputName}_background.png", new object[] { name, alias, "m", "b" });
				AssertFileMatches($"mipmap-mdpi/{outputName}_foreground.png", new object[] { name, alias, "m", "f" });

				AssertFileMatches($"mipmap-xhdpi/{outputName}.png", new object[] { name, alias, "xh", "i" });
				AssertFileMatches($"mipmap-xhdpi/{outputName}_background.png", new object[] { name, alias, "xh", "b" });
				AssertFileMatches($"mipmap-xhdpi/{outputName}_foreground.png", new object[] { name, alias, "xh", "f" });
			}

			[Theory]
			[InlineData("camera.png", "#00FF00", "#00FF00")]
			[InlineData("camera.png", "#00FF00", "#FFFFFF")]
			[InlineData("camera.png", "#00FF00", null)]
			[InlineData("camera.png", "#FFFFFF", "#00FF00")]
			[InlineData("camera.png", "#FFFFFF", "#FFFFFF")]
			[InlineData("camera.png", "#FFFFFF", null)]
			[InlineData("camera.png", null, "#00FF00")]
			[InlineData("camera.png", null, "#FFFFFF")]
			[InlineData("camera.png", null, null)]
			[InlineData("camera.svg", "#00FF00", "#00FF00")]
			[InlineData("camera.svg", "#00FF00", "#FFFFFF")]
			[InlineData("camera.svg", "#00FF00", null)]
			[InlineData("camera.svg", "#FFFFFF", "#00FF00")]
			[InlineData("camera.svg", "#FFFFFF", "#FFFFFF")]
			[InlineData("camera.svg", "#FFFFFF", null)]
			[InlineData("camera.svg", null, "#00FF00")]
			[InlineData("camera.svg", null, "#FFFFFF")]
			[InlineData("camera.svg", null, null)]
			public void SingleAppIconWithColors(string filename, string colorString, string tintColorString)
			{
				var items = new[]
				{
					new TaskItem($"images/{filename}", new Dictionary<string, string>
					{
						["IsAppIcon"] = bool.TrueString,
						["Color"] = colorString,
						["TintColor"] = tintColorString,
					}),
				};

				var task = GetNewTask(items);
				var success = task.Execute();
				Assert.True(success, LogErrorEvents.FirstOrDefault()?.Message);

				var fn = filename.Replace("camera.", "", StringComparison.OrdinalIgnoreCase);
				AssertFileMatches($"mipmap-mdpi/camera.png", new object[] { fn, colorString, tintColorString, "m", "i" });
				AssertFileMatches($"mipmap-mdpi/camera_background.png", new object[] { fn, colorString, tintColorString, "m", "b" });
				AssertFileMatches($"mipmap-mdpi/camera_foreground.png", new object[] { fn, colorString, tintColorString, "m", "f" });
			}

			[Theory]
			[InlineData("camera.png", "#00FF00", "#00FF00")]
			[InlineData("camera.png", "#00FF00", "#FFFFFF")]
			[InlineData("camera.png", "#00FF00", null)]
			[InlineData("camera.png", "#FFFFFF", "#00FF00")]
			[InlineData("camera.png", "#FFFFFF", "#FFFFFF")]
			[InlineData("camera.png", "#FFFFFF", null)]
			[InlineData("camera.png", null, "#00FF00")]
			[InlineData("camera.png", null, "#FFFFFF")]
			[InlineData("camera.png", null, null)]
			[InlineData("camera.svg", "#00FF00", "#00FF00")]
			[InlineData("camera.svg", "#00FF00", "#FFFFFF")]
			[InlineData("camera.svg", "#00FF00", null)]
			[InlineData("camera.svg", "#FFFFFF", "#00FF00")]
			[InlineData("camera.svg", "#FFFFFF", "#FFFFFF")]
			[InlineData("camera.svg", "#FFFFFF", null)]
			[InlineData("camera.svg", null, "#00FF00")]
			[InlineData("camera.svg", null, "#FFFFFF")]
			[InlineData("camera.svg", null, null)]
			public void SingleAppIconGeneratesCorrectFilesWithForegroundScale(string filename, string colorString, string tintColorString)
			{
				var items = new[]
				{
					new TaskItem($"images/{filename}", new Dictionary<string, string>
					{
						["IsAppIcon"] = bool.TrueString,
						["Color"] = colorString,
						["TintColor"] = tintColorString,
						["ForegroundScale"] = "0.5",
					}),
				};

				var task = GetNewTask(items);
				var success = task.Execute();
				Assert.True(success, LogErrorEvents.FirstOrDefault()?.Message);

				var fn = filename.Replace("camera.", "", StringComparison.OrdinalIgnoreCase);
				AssertFileMatches($"mipmap-mdpi/camera.png", new object[] { fn, colorString, tintColorString, "m", "i" });
				AssertFileMatches($"mipmap-mdpi/camera_background.png", new object[] { fn, colorString, tintColorString, "m", "b" });
				AssertFileMatches($"mipmap-mdpi/camera_foreground.png", new object[] { fn, colorString, tintColorString, "m", "f" });
			}

			[Theory]
			[InlineData("camera.png", "#00FF00", "#00FF00")]
			[InlineData("camera.png", "#00FF00", "#FFFFFF")]
			[InlineData("camera.png", "#00FF00", null)]
			[InlineData("camera.png", "#FFFFFF", "#00FF00")]
			[InlineData("camera.png", "#FFFFFF", "#FFFFFF")]
			[InlineData("camera.png", "#FFFFFF", null)]
			[InlineData("camera.png", null, "#00FF00")]
			[InlineData("camera.png", null, "#FFFFFF")]
			[InlineData("camera.png", null, null)]
			[InlineData("camera.svg", "#00FF00", "#00FF00")]
			[InlineData("camera.svg", "#00FF00", "#FFFFFF")]
			[InlineData("camera.svg", "#00FF00", null)]
			[InlineData("camera.svg", "#FFFFFF", "#00FF00")]
			[InlineData("camera.svg", "#FFFFFF", "#FFFFFF")]
			[InlineData("camera.svg", "#FFFFFF", null)]
			[InlineData("camera.svg", null, "#00FF00")]
			[InlineData("camera.svg", null, "#FFFFFF")]
			[InlineData("camera.svg", null, null)]
			public void MultipleAppIconWithColors(string filename, string colorString, string tintColorString)
			{
				var items = new[]
				{
					new TaskItem($"images/dotnet_background.svg", new Dictionary<string, string>
					{
						["ForegroundFile"] = $"images/{filename}",
						["IsAppIcon"] = bool.TrueString,
						["Color"] = colorString,
						["TintColor"] = tintColorString,
					}),
				};

				var task = GetNewTask(items);
				var success = task.Execute();
				Assert.True(success, LogErrorEvents.FirstOrDefault()?.Message);

				var fn = filename.Replace("camera.", "", StringComparison.OrdinalIgnoreCase);
				AssertFileMatches($"mipmap-mdpi/dotnet_background.png", new object[] { fn, colorString, tintColorString, "m", "i" });
				AssertFileMatches($"mipmap-mdpi/dotnet_background_background.png", new object[] { fn, colorString, tintColorString, "m", "b" });
				AssertFileMatches($"mipmap-mdpi/dotnet_background_foreground.png", new object[] { fn, colorString, tintColorString, "m", "f" });
			}

			[Theory]
			[InlineData("camera.png", "#00FF00", "#00FF00")]
			[InlineData("camera.png", "#00FF00", "#FFFFFF")]
			[InlineData("camera.png", "#00FF00", null)]
			[InlineData("camera.png", "#FFFFFF", "#00FF00")]
			[InlineData("camera.png", "#FFFFFF", "#FFFFFF")]
			[InlineData("camera.png", "#FFFFFF", null)]
			[InlineData("camera.png", null, "#00FF00")]
			[InlineData("camera.png", null, "#FFFFFF")]
			[InlineData("camera.png", null, null)]
			[InlineData("camera.svg", "#00FF00", "#00FF00")]
			[InlineData("camera.svg", "#00FF00", "#FFFFFF")]
			[InlineData("camera.svg", "#00FF00", null)]
			[InlineData("camera.svg", "#FFFFFF", "#00FF00")]
			[InlineData("camera.svg", "#FFFFFF", "#FFFFFF")]
			[InlineData("camera.svg", "#FFFFFF", null)]
			[InlineData("camera.svg", null, "#00FF00")]
			[InlineData("camera.svg", null, "#FFFFFF")]
			[InlineData("camera.svg", null, null)]
			public void MultipleAppIconGeneratesCorrectFilesWithForegroundScale(string filename, string colorString, string tintColorString)
			{
				var items = new[]
				{
					new TaskItem($"images/dotnet_background.svg", new Dictionary<string, string>
					{
						["ForegroundFile"] = $"images/{filename}",
						["IsAppIcon"] = bool.TrueString,
						["Color"] = colorString,
						["TintColor"] = tintColorString,
						["ForegroundScale"] = "0.5",
					}),
				};

				var task = GetNewTask(items);
				var success = task.Execute();
				Assert.True(success, LogErrorEvents.FirstOrDefault()?.Message);

				var fn = filename.Replace("camera.", "", StringComparison.OrdinalIgnoreCase);
				AssertFileMatches($"mipmap-mdpi/dotnet_background.png", new object[] { fn, colorString, tintColorString, "m", "i" });
				AssertFileMatches($"mipmap-mdpi/dotnet_background_background.png", new object[] { fn, colorString, tintColorString, "m", "b" });
				AssertFileMatches($"mipmap-mdpi/dotnet_background_foreground.png", new object[] { fn, colorString, tintColorString, "m", "f" });
			}

			[Fact]
			public void DiffPropoprtionWithoutBase()
			{
				var task = GetNewTask(new TaskItem($"images/dotnet_bot.svg"));
				var success = task.Execute();
				Assert.True(success, LogErrorEvents.FirstOrDefault()?.Message);

				AssertFileSize($"drawable-mdpi/dotnet_bot.png", 250, 281);
				AssertFileSize($"drawable-xhdpi/dotnet_bot.png", 500, 562);

				AssertFileMatches($"drawable-mdpi/dotnet_bot.png", new object[] { "mdpi" });
				AssertFileMatches($"drawable-xhdpi/dotnet_bot.png", new object[] { "xhdpi" });
			}

			[Theory]
			[InlineData(1, "dotnet_background.svg", "tall_image.png")]
			[InlineData(1, "dotnet_background.svg", "wide_image.png")]
			[InlineData(1, "tall_image.png", "camera.svg")]
			[InlineData(1, "wide_image.png", "camera.svg")]
			[InlineData(0.5, "dotnet_background.svg", "tall_image.png")]
			[InlineData(0.5, "dotnet_background.svg", "wide_image.png")]
			[InlineData(0.5, "tall_image.png", "camera.svg")]
			[InlineData(0.5, "wide_image.png", "camera.svg")]
			public void DiffPropoprtionAppIconWithoutBaseUseBackgroundSize(double fgScale, string bg, string fg)
			{
				var items = new[]
				{
					new TaskItem($"images/{bg}", new Dictionary<string, string>
					{
						["ForegroundFile"] = $"images/{fg}",
						["IsAppIcon"] = bool.TrueString,
						["ForegroundScale"] = fgScale.ToString(CultureInfo.InvariantCulture),
					}),
				};

				var task = GetNewTask(items);
				var success = task.Execute();
				Assert.True(success, LogErrorEvents.FirstOrDefault()?.Message);

				AssertFileSize($"mipmap-mdpi/{Path.GetFileNameWithoutExtension(bg)}_background.png", 108, 108);
				AssertFileSize($"mipmap-mdpi/{Path.GetFileNameWithoutExtension(bg)}_foreground.png", 108, 108);

				AssertFileSize($"mipmap-xhdpi/{Path.GetFileNameWithoutExtension(bg)}_background.png", 216, 216);
				AssertFileSize($"mipmap-xhdpi/{Path.GetFileNameWithoutExtension(bg)}_foreground.png", 216, 216);

				AssertFileMatches($"mipmap-mdpi/{Path.GetFileNameWithoutExtension(bg)}.png", new object[] { fgScale, bg, fg, "m", "i" });
				AssertFileMatches($"mipmap-mdpi/{Path.GetFileNameWithoutExtension(bg)}_background.png", new object[] { fgScale, bg, fg, "m", "b" });
				AssertFileMatches($"mipmap-mdpi/{Path.GetFileNameWithoutExtension(bg)}_foreground.png", new object[] { fgScale, bg, fg, "m", "f" });

				AssertFileMatches($"mipmap-xhdpi/{Path.GetFileNameWithoutExtension(bg)}.png", new object[] { fgScale, bg, fg, "xh", "i" });
				AssertFileMatches($"mipmap-xhdpi/{Path.GetFileNameWithoutExtension(bg)}_background.png", new object[] { fgScale, bg, fg, "xh", "b" });
				AssertFileMatches($"mipmap-xhdpi/{Path.GetFileNameWithoutExtension(bg)}_foreground.png", new object[] { fgScale, bg, fg, "xh", "f" });
			}

			[Fact]
			public void NonExistantFilesAreDeleted()
			{
				var items = new[]
				{
					new TaskItem("images/camera.svg", new Dictionary<string, string>
					{
						["Link"] = "dog",
					}),
				};

				var task = GetNewTask(items);
				var success = task.Execute();
				Assert.True(success, LogErrorEvents.FirstOrDefault()?.Message);

				AssertFileNotExists("drawable-hdpi/cat.png");
				AssertFileExists("drawable-hdpi/dog.png");

				LogErrorEvents.Clear();
				LogMessageEvents.Clear();

				items = new[]
				{
					new TaskItem("images/camera.svg", new Dictionary<string, string>
					{
						["Link"] = "cat",
					}),
				};

				task = GetNewTask(items);
				success = task.Execute();
				Assert.True(success, LogErrorEvents.FirstOrDefault()?.Message);

				AssertFileNotExists("drawable-hdpi/dot.png");
				AssertFileExists("drawable-hdpi/cat.png");
			}

			//[Theory]
			//[InlineData(1, 1, "dotnet_background.svg", "tall_image.png", 300, 300)]
			//[InlineData(1, 1, "dotnet_background.svg", "wide_image.png", 300, 300)]
			//[InlineData(0.5, 1, "dotnet_background.svg", "tall_image.png", 150, 150)]
			//[InlineData(0.5, 1, "dotnet_background.svg", "wide_image.png", 150, 150)]
			//[InlineData(1, 0.5, "dotnet_background.svg", "tall_image.png", 300, 300)]
			//[InlineData(1, 0.5, "dotnet_background.svg", "wide_image.png", 300, 300)]
			//[InlineData(1, 1, "tall_image.png", "camera.svg", 300, 300)]
			//[InlineData(0.5, 1, "tall_image.png", "camera.svg", 150, 150)]
			//[InlineData(1, 0.5, "tall_image.png", "camera.svg", 300, 300)]
			//[InlineData(1, 1, "wide_image.png", "camera.svg", 300, 300)]
			//[InlineData(0.5, 1, "wide_image.png", "camera.svg", 150, 150)]
			//[InlineData(1, 0.5, "wide_image.png", "camera.svg", 300, 300)]
			//public void DiffPropoprtionWithBaseSize(double dpi, double fgScale, string bg, string fg, int exWidth, int exHeight)
			//{
			//	var info = new ResizeImageInfo
			//	{
			//		Filename = "images/" + bg,
			//		ForegroundFilename = "images/" + fg,
			//		ForegroundScale = fgScale,
			//		IsAppIcon = true,
			//		Color = SKColors.Orange,
			//		BaseSize = new SKSize(300, 300),
			//	};

			//	var tools = new SkiaSharpAppIconTools(info, Logger);
			//	var dpiPath = new DpiPath("", (decimal)dpi);

			//	tools.Resize(dpiPath, DestinationFilename);

			//	AssertFileSize(DestinationFilename, exWidth, exHeight);
			//}

			//[Theory]
			//[InlineData(1, 1, "dotnet_background.svg", "tall_image.png", 300, 300)]
			//[InlineData(1, 1, "dotnet_background.svg", "wide_image.png", 300, 300)]
			//[InlineData(0.5, 1, "dotnet_background.svg", "tall_image.png", 150, 150)]
			//[InlineData(0.5, 1, "dotnet_background.svg", "wide_image.png", 150, 150)]
			//[InlineData(1, 0.5, "dotnet_background.svg", "tall_image.png", 300, 300)]
			//[InlineData(1, 0.5, "dotnet_background.svg", "wide_image.png", 300, 300)]
			//[InlineData(1, 1, "tall_image.png", "camera.svg", 300, 300)]
			//[InlineData(0.5, 1, "tall_image.png", "camera.svg", 150, 150)]
			//[InlineData(1, 0.5, "tall_image.png", "camera.svg", 300, 300)]
			//[InlineData(1, 1, "wide_image.png", "camera.svg", 300, 300)]
			//[InlineData(0.5, 1, "wide_image.png", "camera.svg", 150, 150)]
			//[InlineData(1, 0.5, "wide_image.png", "camera.svg", 300, 300)]
			//public void DiffPropoprtionWithDpiSize(double dpi, double fgScale, string bg, string fg, int exWidth, int exHeight)
			//{
			//	var info = new ResizeImageInfo
			//	{
			//		Filename = "images/" + bg,
			//		ForegroundFilename = "images/" + fg,
			//		ForegroundScale = fgScale,
			//		IsAppIcon = true,
			//		Color = SKColors.Orange,
			//	};

			//	var tools = new SkiaSharpAppIconTools(info, Logger);
			//	var dpiPath = new DpiPath("", (decimal)dpi, size: new SKSize(300, 300));

			//	tools.Resize(dpiPath, DestinationFilename);

			//	AssertFileSize(DestinationFilename, exWidth, exHeight);
			//}
		}

		public class ExecuteForiOS : ExecuteForPlatformApp
		{
			protected override string Platform => "ios";

			protected override string GetPlatformOutputFileName(string file) => $"{file}";

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
			public void TwoImagesWithOnlyPathSucceed()
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
				AssertFileSize("camera_color.png", 256, 256);

				AssertFileSize("camera@2x.png", 3584, 3584);
				AssertFileSize("camera_color@2x.png", 512, 512);
			}

			[Fact]
			public void ImageWithOnlyPathHasMetadata()
			{
				var items = new[]
				{
					new TaskItem("images/camera.png", ResizeMetadata),
				};

				var task = GetNewTask(items);
				var success = task.Execute();
				Assert.True(success, LogErrorEvents.FirstOrDefault()?.Message);

				var copied = task.CopiedResources;
				Assert.Equal(items.Length * DpiPath.Ios.Image.Length, copied.Length);

				var mdpi = GetCopiedResource(task, "camera.png");
				Assert.Equal("", mdpi.GetMetadata("_ResizetizerDpiPath"));
				Assert.Equal("1.0", mdpi.GetMetadata("_ResizetizerDpiScale"));

				var xhdpi = GetCopiedResource(task, "camera@2x.png");
				Assert.Equal("", xhdpi.GetMetadata("_ResizetizerDpiPath"));
				Assert.Equal("2.0", xhdpi.GetMetadata("_ResizetizerDpiScale"));
			}

			[Fact]
			public void TwoImagesWithOnlyPathHasMetadata()
			{
				var items = new[]
				{
					new TaskItem("images/camera.png", ResizeMetadata),
					new TaskItem("images/camera_color.png", ResizeMetadata),
				};

				var task = GetNewTask(items);
				var success = task.Execute();
				Assert.True(success, LogErrorEvents.FirstOrDefault()?.Message);

				var copied = task.CopiedResources;
				Assert.Equal(items.Length * DpiPath.Ios.Image.Length, copied.Length);

				var mdpi = GetCopiedResource(task, "camera.png");
				Assert.Equal("", mdpi.GetMetadata("_ResizetizerDpiPath"));
				Assert.Equal("1.0", mdpi.GetMetadata("_ResizetizerDpiScale"));

				var xhdpi = GetCopiedResource(task, "camera@2x.png");
				Assert.Equal("", xhdpi.GetMetadata("_ResizetizerDpiPath"));
				Assert.Equal("2.0", xhdpi.GetMetadata("_ResizetizerDpiScale"));

				mdpi = GetCopiedResource(task, "camera_color.png");
				Assert.Equal("", mdpi.GetMetadata("_ResizetizerDpiPath"));
				Assert.Equal("1.0", mdpi.GetMetadata("_ResizetizerDpiScale"));

				xhdpi = GetCopiedResource(task, "camera_color@2x.png");
				Assert.Equal("", xhdpi.GetMetadata("_ResizetizerDpiPath"));
				Assert.Equal("2.0", xhdpi.GetMetadata("_ResizetizerDpiScale"));
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

			[Theory]
			[InlineData("camera", null, "camera")]
			[InlineData("camera", "", "camera")]
			[InlineData("camera", "camera", "camera")]
			[InlineData("camera", "camera.png", "camera")]
			[InlineData("camera", "folder/camera.png", "camera")]
			[InlineData("camera", "the_alias", "the_alias")]
			[InlineData("camera", "the_alias.png", "the_alias")]
			[InlineData("camera", "folder/the_alias.png", "the_alias")]
			[InlineData("camera_color", null, "camera_color")]
			[InlineData("camera_color", "", "camera_color")]
			[InlineData("camera_color", "camera_color", "camera_color")]
			[InlineData("camera_color", "camera_color.png", "camera_color")]
			[InlineData("camera_color", "folder/camera_color.png", "camera_color")]
			[InlineData("camera_color", "the_alias", "the_alias")]
			[InlineData("camera_color", "the_alias.png", "the_alias")]
			[InlineData("camera_color", "folder/the_alias.png", "the_alias")]
			public void SingleRasterAppIconWithOnlyPathSucceedsWithoutVectors(string name, string alias, string outputName)
			{
				var items = new[]
				{
					new TaskItem($"images/{name}.png", new Dictionary<string, string>
					{
						["IsAppIcon"] = bool.TrueString,
						["Link"] = alias,
					}),
				};

				var task = GetNewTask(items);
				var success = task.Execute();
				Assert.True(success, LogErrorEvents.FirstOrDefault()?.Message);

				AssertFileSize($"Assets.xcassets/{outputName}.appiconset/{outputName}20x20@2x.png", 40, 40);
				AssertFileSize($"Assets.xcassets/{outputName}.appiconset/{outputName}20x20@3x.png", 60, 60);
				AssertFileSize($"Assets.xcassets/{outputName}.appiconset/{outputName}60x60@2x.png", 120, 120);
				AssertFileSize($"Assets.xcassets/{outputName}.appiconset/{outputName}60x60@3x.png", 180, 180);
				AssertFileSize($"Assets.xcassets/{outputName}.appiconset/{outputName}ItunesArtwork.png", 1024, 1024);

				AssertFileExists($"Assets.xcassets/{outputName}.appiconset/Contents.json");

				AssertFileContains($"Assets.xcassets/{outputName}.appiconset/Contents.json",
					$"\"filename\": \"{outputName}20x20@2x.png\"",
					$"\"size\": \"20x20\",");
			}

			[Theory]
			[InlineData("appicon", null, "appicon")]
			[InlineData("appicon", "", "appicon")]
			[InlineData("appicon", "appicon", "appicon")]
			[InlineData("appicon", "appicon.png", "appicon")]
			[InlineData("appicon", "folder/appicon.png", "appicon")]
			[InlineData("appicon", "the_alias", "the_alias")]
			[InlineData("appicon", "the_alias.png", "the_alias")]
			[InlineData("appicon", "folder/the_alias.png", "the_alias")]
			[InlineData("camera", null, "camera")]
			[InlineData("camera", "", "camera")]
			[InlineData("camera", "camera", "camera")]
			[InlineData("camera", "camera.png", "camera")]
			[InlineData("camera", "folder/camera.png", "camera")]
			[InlineData("camera", "the_alias", "the_alias")]
			[InlineData("camera", "the_alias.png", "the_alias")]
			[InlineData("camera", "folder/the_alias.png", "the_alias")]
			[InlineData("camera_color", null, "camera_color")]
			[InlineData("camera_color", "", "camera_color")]
			[InlineData("camera_color", "camera_color", "camera_color")]
			[InlineData("camera_color", "camera_color.png", "camera_color")]
			[InlineData("camera_color", "folder/camera_color.png", "camera_color")]
			[InlineData("camera_color", "the_alias", "the_alias")]
			[InlineData("camera_color", "the_alias.png", "the_alias")]
			[InlineData("camera_color", "folder/the_alias.png", "the_alias")]
			public void SingleVectorAppIconWithOnlyPathSucceedsWithVectors(string name, string alias, string outputName)
			{
				var items = new[]
				{
					new TaskItem($"images/{name}.svg", new Dictionary<string, string>
					{
						["IsAppIcon"] = bool.TrueString,
						["Link"] = alias,
					}),
				};

				var task = GetNewTask(items);
				var success = task.Execute();
				Assert.True(success, LogErrorEvents.FirstOrDefault()?.Message);

				AssertFileSize($"Assets.xcassets/{outputName}.appiconset/{outputName}20x20@2x.png", 40, 40);
				AssertFileSize($"Assets.xcassets/{outputName}.appiconset/{outputName}20x20@3x.png", 60, 60);
				AssertFileSize($"Assets.xcassets/{outputName}.appiconset/{outputName}60x60@2x.png", 120, 120);
				AssertFileSize($"Assets.xcassets/{outputName}.appiconset/{outputName}60x60@3x.png", 180, 180);
				AssertFileSize($"Assets.xcassets/{outputName}.appiconset/{outputName}ItunesArtwork.png", 1024, 1024);

				AssertFileExists($"Assets.xcassets/{outputName}.appiconset/Contents.json");

				AssertFileContains($"Assets.xcassets/{outputName}.appiconset/Contents.json",
					$"\"filename\": \"{outputName}20x20@2x.png\"",
					$"\"size\": \"20x20\",");
			}

			[Fact]
			public void NonExistantFilesAreDeleted()
			{
				var items = new[]
				{
					new TaskItem("images/camera.svg", new Dictionary<string, string>
					{
						["Link"] = "dog",
					}),
				};

				var task = GetNewTask(items);
				var success = task.Execute();
				Assert.True(success, LogErrorEvents.FirstOrDefault()?.Message);

				AssertFileNotExists("cat.png");
				AssertFileExists("dog.png");

				LogErrorEvents.Clear();
				LogMessageEvents.Clear();

				items = new[]
				{
					new TaskItem("images/camera.svg", new Dictionary<string, string>
					{
						["Link"] = "cat",
					}),
				};

				task = GetNewTask(items);
				success = task.Execute();
				Assert.True(success, LogErrorEvents.FirstOrDefault()?.Message);

				AssertFileNotExists("dot.png");
				AssertFileExists("cat.png");
			}
		}

		public class ExecuteForWindows : ExecuteForPlatformApp
		{
			protected override string Platform => "uwp";

			protected override string GetPlatformOutputFileName(string file) =>
				$"{Path.GetFileNameWithoutExtension(file)}.scale-100{Path.GetExtension(file)}";

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

				AssertFileSize("camera.scale-100.png", 1792, 1792);
				AssertFileSize("camera.scale-200.png", 3584, 3584);
			}

			[Fact]
			public void TwoImagesWithOnlyPathSucceed()
			{
				var items = new[]
				{
					new TaskItem("images/camera.png", ResizeMetadata),
					new TaskItem("images/camera_color.png", ResizeMetadata),
				};

				var task = GetNewTask(items);
				var success = task.Execute();
				Assert.True(success, LogErrorEvents.FirstOrDefault()?.Message);

				AssertFileSize("camera.scale-100.png", 1792, 1792);
				AssertFileSize("camera_color.scale-100.png", 256, 256);

				AssertFileSize("camera.scale-200.png", 3584, 3584);
				AssertFileSize("camera_color.scale-200.png", 512, 512);
			}

			[Fact]
			public void ImageWithOnlyPathHasMetadata()
			{
				var items = new[]
				{
					new TaskItem("images/camera.png", ResizeMetadata),
				};

				var task = GetNewTask(items);
				var success = task.Execute();
				Assert.True(success, LogErrorEvents.FirstOrDefault()?.Message);

				var copied = task.CopiedResources;
				Assert.Equal(items.Length * DpiPath.Windows.Image.Length, copied.Length);

				var mdpi = GetCopiedResource(task, "camera.scale-100.png");
				Assert.Equal("", mdpi.GetMetadata("_ResizetizerDpiPath"));
				Assert.Equal("1.0", mdpi.GetMetadata("_ResizetizerDpiScale"));

				var xhdpi = GetCopiedResource(task, "camera.scale-200.png");
				Assert.Equal("", xhdpi.GetMetadata("_ResizetizerDpiPath"));
				Assert.Equal("2.0", xhdpi.GetMetadata("_ResizetizerDpiScale"));
			}

			[Fact]
			public void TwoImagesWithOnlyPathHasMetadata()
			{
				var items = new[]
				{
					new TaskItem("images/camera.png", ResizeMetadata),
					new TaskItem("images/camera_color.png", ResizeMetadata),
				};

				var task = GetNewTask(items);
				var success = task.Execute();
				Assert.True(success, LogErrorEvents.FirstOrDefault()?.Message);

				var copied = task.CopiedResources;
				Assert.Equal(items.Length * DpiPath.Windows.Image.Length, copied.Length);

				var mdpi = GetCopiedResource(task, "camera.scale-100.png");
				Assert.Equal("", mdpi.GetMetadata("_ResizetizerDpiPath"));
				Assert.Equal("1.0", mdpi.GetMetadata("_ResizetizerDpiScale"));

				var xhdpi = GetCopiedResource(task, "camera.scale-200.png");
				Assert.Equal("", xhdpi.GetMetadata("_ResizetizerDpiPath"));
				Assert.Equal("2.0", xhdpi.GetMetadata("_ResizetizerDpiScale"));

				mdpi = GetCopiedResource(task, "camera_color.scale-100.png");
				Assert.Equal("", mdpi.GetMetadata("_ResizetizerDpiPath"));
				Assert.Equal("1.0", mdpi.GetMetadata("_ResizetizerDpiScale"));

				xhdpi = GetCopiedResource(task, "camera_color.scale-200.png");
				Assert.Equal("", xhdpi.GetMetadata("_ResizetizerDpiPath"));
				Assert.Equal("2.0", xhdpi.GetMetadata("_ResizetizerDpiScale"));
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

				AssertFileSize($"{outputName}.scale-100.png", 44, 44);
				AssertFileSize($"{outputName}.scale-200.png", 88, 88);
			}

			[Theory]
			[InlineData("camera", null, "camera")]
			[InlineData("camera", "", "camera")]
			[InlineData("camera", "camera", "camera")]
			[InlineData("camera", "camera.png", "camera")]
			[InlineData("camera", "folder/camera.png", "camera")]
			[InlineData("camera", "the_alias", "the_alias")]
			[InlineData("camera", "the_alias.png", "the_alias")]
			[InlineData("camera", "folder/the_alias.png", "the_alias")]
			[InlineData("camera_color", null, "camera_color")]
			[InlineData("camera_color", "", "camera_color")]
			[InlineData("camera_color", "camera_color", "camera_color")]
			[InlineData("camera_color", "camera_color.png", "camera_color")]
			[InlineData("camera_color", "folder/camera_color.png", "camera_color")]
			[InlineData("camera_color", "the_alias", "the_alias")]
			[InlineData("camera_color", "the_alias.png", "the_alias")]
			[InlineData("camera_color", "folder/the_alias.png", "the_alias")]
			public void SingleRasterAppIconWithOnlyPathSucceedsWithoutVectors(string name, string alias, string outputName)
			{
				var items = new[]
				{
					new TaskItem($"images/{name}.png", new Dictionary<string, string>
					{
						["IsAppIcon"] = bool.TrueString,
						["Link"] = alias,
					}),
				};

				var task = GetNewTask(items);
				var success = task.Execute();
				Assert.True(success, LogErrorEvents.FirstOrDefault()?.Message);

				AssertFileSize($"{outputName}Logo.scale-100.png", 44, 44);
				AssertFileSize($"{outputName}Logo.scale-125.png", 55, 55);
				AssertFileSize($"{outputName}Logo.scale-200.png", 88, 88);

				AssertFileSize($"{outputName}StoreLogo.scale-100.png", 50, 50);
				AssertFileSize($"{outputName}StoreLogo.scale-200.png", 100, 100);

				AssertFileSize($"{outputName}MediumTile.scale-100.png", 150, 150);
				AssertFileSize($"{outputName}MediumTile.scale-150.png", 225, 225);
			}

			[Theory]
			[InlineData("appicon", null, "appicon")]
			[InlineData("appicon", "", "appicon")]
			[InlineData("appicon", "appicon", "appicon")]
			[InlineData("appicon", "appicon.png", "appicon")]
			[InlineData("appicon", "folder/appicon.png", "appicon")]
			[InlineData("appicon", "the_alias", "the_alias")]
			[InlineData("appicon", "the_alias.png", "the_alias")]
			[InlineData("appicon", "folder/the_alias.png", "the_alias")]
			[InlineData("camera", null, "camera")]
			[InlineData("camera", "", "camera")]
			[InlineData("camera", "camera", "camera")]
			[InlineData("camera", "camera.png", "camera")]
			[InlineData("camera", "folder/camera.png", "camera")]
			[InlineData("camera", "the_alias", "the_alias")]
			[InlineData("camera", "the_alias.png", "the_alias")]
			[InlineData("camera", "folder/the_alias.png", "the_alias")]
			[InlineData("camera_color", null, "camera_color")]
			[InlineData("camera_color", "", "camera_color")]
			[InlineData("camera_color", "camera_color", "camera_color")]
			[InlineData("camera_color", "camera_color.png", "camera_color")]
			[InlineData("camera_color", "folder/camera_color.png", "camera_color")]
			[InlineData("camera_color", "the_alias", "the_alias")]
			[InlineData("camera_color", "the_alias.png", "the_alias")]
			[InlineData("camera_color", "folder/the_alias.png", "the_alias")]
			public void SingleVectorAppIconWithOnlyPathSucceedsWithVectors(string name, string alias, string outputName)
			{
				var items = new[]
				{
					new TaskItem($"images/{name}.svg", new Dictionary<string, string>
					{
						["IsAppIcon"] = bool.TrueString,
						["Link"] = alias,
					}),
				};

				var task = GetNewTask(items);
				var success = task.Execute();
				Assert.True(success, LogErrorEvents.FirstOrDefault()?.Message);

				AssertFileSize($"{outputName}Logo.scale-100.png", 44, 44);
				AssertFileSize($"{outputName}Logo.scale-125.png", 55, 55);
				AssertFileSize($"{outputName}Logo.scale-200.png", 88, 88);

				AssertFileSize($"{outputName}StoreLogo.scale-100.png", 50, 50);
				AssertFileSize($"{outputName}StoreLogo.scale-200.png", 100, 100);

				AssertFileSize($"{outputName}MediumTile.scale-100.png", 150, 150);
				AssertFileSize($"{outputName}MediumTile.scale-150.png", 225, 225);
			}

			[Theory]
			[InlineData("dotnet_logo", "dotnet_background")]
			public void AppIconWithBackgroundSucceedsWithVectors(string fg, string bg)
			{
				var items = new[]
				{
					new TaskItem($"images/{bg}.svg", new Dictionary<string, string>
					{
						["IsAppIcon"] = bool.TrueString,
						["ForegroundFile"] = $"images/{fg}.svg",
						["Color"] = $"#512BD4",
					}),
				};

				var task = GetNewTask(items);
				var success = task.Execute();
				Assert.True(success, LogErrorEvents.FirstOrDefault()?.Message);

				AssertFileSize($"{bg}Logo.scale-100.png", 44, 44);
				AssertFileSize($"{bg}Logo.scale-125.png", 55, 55);
				AssertFileSize($"{bg}Logo.scale-200.png", 88, 88);

				AssertFileSize($"{bg}StoreLogo.scale-100.png", 50, 50);
				AssertFileSize($"{bg}StoreLogo.scale-200.png", 100, 100);

				AssertFileSize($"{bg}MediumTile.scale-100.png", 150, 150);
				AssertFileSize($"{bg}MediumTile.scale-150.png", 225, 225);

				AssertFileSize($"{bg}WideTile.scale-100.png", 310, 150);
				AssertFileSize($"{bg}WideTile.scale-200.png", 620, 300);
			}

			[Fact]
			public void ColorsInCssCanBeUsed()
			{
				var items = new[]
				{
					new TaskItem($"images/not_working.svg"),
				};

				var task = GetNewTask(items);
				var success = task.Execute();
				Assert.True(success, LogErrorEvents.FirstOrDefault()?.Message);

				AssertFileSize("not_working.scale-100.png", 24, 24);

				AssertFileContains("not_working.scale-100.png", 0xFF71559B, 2, 6);
			}

			[Fact]
			public void NonExistantFilesAreDeleted()
			{
				var items = new[]
				{
					new TaskItem("images/camera.svg", new Dictionary<string, string>
					{
						["Link"] = "dog",
					}),
				};

				var task = GetNewTask(items);
				var success = task.Execute();
				Assert.True(success, LogErrorEvents.FirstOrDefault()?.Message);

				AssertFileNotExists("cat.scale-150.png");
				AssertFileExists("dog.scale-150.png");

				LogErrorEvents.Clear();
				LogMessageEvents.Clear();

				items = new[]
				{
					new TaskItem("images/camera.svg", new Dictionary<string, string>
					{
						["Link"] = "cat",
					}),
				};

				task = GetNewTask(items);
				success = task.Execute();
				Assert.True(success, LogErrorEvents.FirstOrDefault()?.Message);

				AssertFileNotExists("dot.scale-150.png");
				AssertFileExists("cat.scale-150.png");
			}
		}

		public class ExecuteForAny : ExecuteForApp
		{
			[Theory]
			[InlineData("image.svg", "100,100", true)]
			[InlineData("image.png", "100,100", true)]
			[InlineData("image.jpg", "100,100", true)]
			[InlineData("image.svg", "100;100", true)]
			[InlineData("image.png", "100;100", true)]
			[InlineData("image.jpg", "100;100", true)]
			[InlineData("image.svg", null, true)]
			[InlineData("image.png", null, false)]
			[InlineData("image.jpg", null, false)]
			public void ShouldResize(string filename, string baseSize, bool resize)
			{
				Directory.CreateDirectory(DestinationDirectory);
				var path = Path.Combine(DestinationDirectory, filename);
				File.WriteAllText(path, contents: "");
				var item = new TaskItem(path);
				if (!string.IsNullOrEmpty(baseSize))
				{
					item.SetMetadata("BaseSize", baseSize);
				}
				var size = ResizeImageInfo.Parse(item);
				Assert.Equal(resize, size.Resize);
			}
		}
	}
}
