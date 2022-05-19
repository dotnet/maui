using System;
using System.Collections.Generic;
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

			public ExecuteForApp(string type)
				: base(Path.Combine(Path.GetTempPath(), "ResizetizeImagesTests", type, Path.GetRandomFileName()))
			{
			}

			protected ResizetizeImages GetNewTask(string type, params ITaskItem[] items) =>
				new ResizetizeImages
				{
					PlatformType = type,
					IntermediateOutputPath = DestinationDirectory,
					InputsFile = "mauiimage.inputs",
					Images = items,
					BuildEngine = this,
				};

			protected ITaskItem GetCopiedResource(ResizetizeImages task, string path) =>
				task.CopiedResources.Single(c => c.ItemSpec.Replace('\\', '/').EndsWith(path, StringComparison.Ordinal));

			protected void AssertFileSize(string file, int width, int height)
			{
				file = Path.Combine(DestinationDirectory, file);

				Assert.True(File.Exists(file), $"File did not exist: {file}");

				using var codec = SKCodec.Create(file);
				Assert.Equal(width, codec.Info.Width);
				Assert.Equal(height, codec.Info.Height);
			}

			protected void AssertFileExists(string file)
			{
				file = Path.Combine(DestinationDirectory, file);

				Assert.True(File.Exists(file), $"File did not exist: {file}");
			}

			protected void AssertFileNotExists(string file)
			{
				file = Path.Combine(DestinationDirectory, file);

				Assert.False(File.Exists(file), $"File existed: {file}");
			}

			protected void AssertFileContains(string file, params string[] snippet)
			{
				file = Path.Combine(DestinationDirectory, file);

				var content = File.ReadAllText(file);

				foreach (var snip in snippet)
					Assert.Contains(snip, content, StringComparison.Ordinal);
			}

			protected void AssertFileContains(string file, SKColor color, int x, int y)
			{
				file = Path.Combine(DestinationDirectory, file);

				using var resultImage = SKBitmap.Decode(file);
				using var pixmap = resultImage.PeekPixels();
				Assert.Equal(color, pixmap.GetPixelColor(x, y));
			}
		}

		public class ExecuteForAndroid : ExecuteForApp
		{
			public ExecuteForAndroid()
				: base("Android")
			{
			}

			ResizetizeImages GetNewTask(params ITaskItem[] items) =>
				GetNewTask("android", items);

			[Fact]
			public void NoItemsSucceed()
			{
				var task = GetNewTask();

				var success = task.Execute();

				Assert.True(success);
			}

			[Fact]
			public void NullItemsSucceed()
			{
				var task = GetNewTask(null);

				var success = task.Execute();

				Assert.True(success);
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

				Assert.True(success);
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

				Assert.True(success);
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
				Assert.True(success);

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
				Assert.True(success);

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
				Assert.True(success);

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
				Assert.True(success);

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
				Assert.True(success);

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
				Assert.True(success);

				AssertFileSize("drawable/camera.png", 1792, 1792);
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
				Assert.True(success);

				AssertFileExists("drawable/camera.xml");
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
				Assert.True(success);

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
			public void SingleRasterAppIconWithOnlyPathSucceedsWithVectors(string name, string alias, string outputName)
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
				task.AllowVectorAdaptiveIcons = true;
				var success = task.Execute();
				Assert.True(success);

				AssertFileSize($"mipmap-mdpi/{outputName}.png", 48, 48);
				AssertFileSize($"mipmap-mdpi/{outputName}_background.png", 108, 108);
				AssertFileNotExists($"mipmap-mdpi/{outputName}_foreground.png");

				AssertFileSize($"mipmap-xhdpi/{outputName}.png", 96, 96);
				AssertFileSize($"mipmap-xhdpi/{outputName}_background.png", 216, 216);
				AssertFileNotExists($"mipmap-xhdpi/{outputName}_foreground.png");

				AssertFileExists($"drawable/{outputName}_foreground.xml");
				AssertFileNotExists($"drawable-v24/{outputName}_background.xml");

				AssertFileExists($"mipmap-anydpi-v26/{outputName}.xml");
				AssertFileExists($"mipmap-anydpi-v26/{outputName}_round.xml");

				AssertFileContains($"mipmap-anydpi-v26/{outputName}.xml",
					$"<foreground android:drawable=\"@drawable/{outputName}_foreground\"/>",
					$"<background android:drawable=\"@mipmap/{outputName}_background\"/>");

				AssertFileContains($"mipmap-anydpi-v26/{outputName}_round.xml",
					$"<foreground android:drawable=\"@drawable/{outputName}_foreground\"/>",
					$"<background android:drawable=\"@mipmap/{outputName}_background\"/>");
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
				task.AllowVectorAdaptiveIcons = true;
				var success = task.Execute();
				Assert.True(success);

				AssertFileSize($"mipmap-mdpi/{outputName}.png", 48, 48);
				AssertFileNotExists($"mipmap-mdpi/{outputName}_background.png");
				AssertFileNotExists($"mipmap-mdpi/{outputName}_foreground.png");

				AssertFileSize($"mipmap-xhdpi/{outputName}.png", 96, 96);
				AssertFileNotExists($"mipmap-xhdpi/{outputName}_background.png");
				AssertFileNotExists($"mipmap-xhdpi/{outputName}_foreground.png");

				AssertFileExists($"drawable/{outputName}_foreground.xml");
				AssertFileExists($"drawable-v24/{outputName}_background.xml");

				AssertFileExists($"mipmap-anydpi-v26/{outputName}.xml");
				AssertFileExists($"mipmap-anydpi-v26/{outputName}_round.xml");

				AssertFileContains($"mipmap-anydpi-v26/{outputName}.xml",
					$"<foreground android:drawable=\"@drawable/{outputName}_foreground\"/>",
					$"<background android:drawable=\"@drawable/{outputName}_background\"/>");

				AssertFileContains($"mipmap-anydpi-v26/{outputName}_round.xml",
					$"<foreground android:drawable=\"@drawable/{outputName}_foreground\"/>",
					$"<background android:drawable=\"@drawable/{outputName}_background\"/>");
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
				Assert.True(success);

				AssertFileSize($"mipmap-mdpi/{outputName}.png", 48, 48);
				AssertFileSize($"mipmap-mdpi/{outputName}_background.png", 108, 108);
				AssertFileNotExists($"mipmap-mdpi/{outputName}_foreground.png");

				AssertFileSize($"mipmap-xhdpi/{outputName}.png", 96, 96);
				AssertFileSize($"mipmap-xhdpi/{outputName}_background.png", 216, 216);
				AssertFileNotExists($"mipmap-xhdpi/{outputName}_foreground.png");

				AssertFileExists($"drawable/{outputName}_foreground.xml");
				AssertFileNotExists($"drawable-v24/{outputName}_background.xml");

				AssertFileExists($"mipmap-anydpi-v26/{outputName}.xml");
				AssertFileExists($"mipmap-anydpi-v26/{outputName}_round.xml");

				AssertFileContains($"mipmap-anydpi-v26/{outputName}.xml",
					$"<foreground android:drawable=\"@drawable/{outputName}_foreground\"/>",
					$"<background android:drawable=\"@mipmap/{outputName}_background\"/>");

				AssertFileContains($"mipmap-anydpi-v26/{outputName}_round.xml",
					$"<foreground android:drawable=\"@drawable/{outputName}_foreground\"/>",
					$"<background android:drawable=\"@mipmap/{outputName}_background\"/>");
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
				Assert.True(success);

				AssertFileSize($"mipmap-mdpi/{outputName}.png", 48, 48);
				AssertFileSize($"mipmap-mdpi/{outputName}_background.png", 108, 108);
				AssertFileNotExists($"mipmap-mdpi/{outputName}_foreground.png");

				AssertFileSize($"mipmap-xhdpi/{outputName}.png", 96, 96);
				AssertFileSize($"mipmap-xhdpi/{outputName}_background.png", 216, 216);
				AssertFileNotExists($"mipmap-xhdpi/{outputName}_foreground.png");

				AssertFileExists($"drawable/{outputName}_foreground.xml");
				AssertFileNotExists($"drawable-v24/{outputName}_background.xml");

				AssertFileExists($"mipmap-anydpi-v26/{outputName}.xml");
				AssertFileExists($"mipmap-anydpi-v26/{outputName}_round.xml");

				AssertFileContains($"mipmap-anydpi-v26/{outputName}.xml",
					$"<foreground android:drawable=\"@drawable/{outputName}_foreground\"/>",
					$"<background android:drawable=\"@mipmap/{outputName}_background\"/>");

				AssertFileContains($"mipmap-anydpi-v26/{outputName}_round.xml",
					$"<foreground android:drawable=\"@drawable/{outputName}_foreground\"/>",
					$"<background android:drawable=\"@mipmap/{outputName}_background\"/>");
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
				Assert.True(success);

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
			}
		}

		public class ExecuteForiOS : ExecuteForApp
		{
			public ExecuteForiOS()
				: base("iOS")
			{
			}

			ResizetizeImages GetNewTask(params ITaskItem[] items) =>
				GetNewTask("ios", items);

			[Fact]
			public void NoItemsSucceed()
			{
				var task = GetNewTask();

				var success = task.Execute();

				Assert.True(success);
			}

			[Fact]
			public void NullItemsSucceed()
			{
				var task = GetNewTask(null);

				var success = task.Execute();

				Assert.True(success);
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

				Assert.True(success);
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
				Assert.True(success);

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
				Assert.True(success);

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
				Assert.True(success);

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
				Assert.True(success);

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
				Assert.True(success);

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
				Assert.True(success);

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
				Assert.True(success);

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
		}

		public class ExecuteForWindows : ExecuteForApp
		{
			public ExecuteForWindows()
				: base("Windows")
			{
			}

			ResizetizeImages GetNewTask(params ITaskItem[] items) =>
				GetNewTask("uwp", items);

			[Fact]
			public void NoItemsSucceed()
			{
				var task = GetNewTask();

				var success = task.Execute();

				Assert.True(success);
			}

			[Fact]
			public void NullItemsSucceed()
			{
				var task = GetNewTask(null);

				var success = task.Execute();

				Assert.True(success);
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

				Assert.True(success);
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
				Assert.True(success);

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
				Assert.True(success);

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
				Assert.True(success);

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
				Assert.True(success);

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
				Assert.True(success);

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
				Assert.True(success);

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
				Assert.True(success);

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
				Assert.True(success);

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
				Assert.True(success);

				AssertFileSize("not_working.scale-100.png", 24, 24);

				AssertFileContains("not_working.scale-100.png", 0xFF71559B, 2, 6);
			}
		}

		public class ExecuteForAny : ExecuteForApp
		{
			public ExecuteForAny() : base("Any") { }

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
