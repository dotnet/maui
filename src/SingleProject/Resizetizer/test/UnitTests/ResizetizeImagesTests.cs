using System;
using System.Collections;
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
				task.CopiedResources.Single(c => c.ItemSpec.Replace("\\", "/").EndsWith(path));

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

			protected void AssertFileContains(string file, params string[] snippet)
			{
				file = Path.Combine(DestinationDirectory, file);

				var content = File.ReadAllText(file);

				foreach (var snip in snippet)
					Assert.Contains(snip, content);
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
					new TaskItem("images/camera.png"),
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
					new TaskItem("images/camera.png"),
					new TaskItem("images/camera_color.png"),
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
					new TaskItem("images/camera.png"),
				};

				var task = GetNewTask(items);
				var success = task.Execute();
				Assert.True(success);

				var copied = task.CopiedResources;
				Assert.Equal(items.Length * DpiPath.Android.Length, copied.Length);

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
					new TaskItem("images/camera.png"),
					new TaskItem("images/camera_color.png"),
				};

				var task = GetNewTask(items);
				var success = task.Execute();
				Assert.True(success);

				var copied = task.CopiedResources;
				Assert.Equal(items.Length * DpiPath.Android.Length, copied.Length);

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
				AssertFileSize($"mipmap-xhdpi/{outputName}.png", 96, 96);

				var vectors = Directory.GetFiles(DestinationDirectory, "*.xml", SearchOption.AllDirectories);
				Assert.Empty(vectors);
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
				var success = task.Execute();
				Assert.True(success);

				AssertFileSize($"mipmap-mdpi/{outputName}.png", 48, 48);
				AssertFileSize($"mipmap-xhdpi/{outputName}.png", 96, 96);

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
					new TaskItem("images/camera.png"),
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
					new TaskItem("images/camera.png"),
					new TaskItem("images/camera_color.png"),
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
					new TaskItem("images/camera.png"),
				};

				var task = GetNewTask(items);
				var success = task.Execute();
				Assert.True(success);

				var copied = task.CopiedResources;
				Assert.Equal(items.Length * DpiPath.Ios.Length, copied.Length);

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
					new TaskItem("images/camera.png"),
					new TaskItem("images/camera_color.png"),
				};

				var task = GetNewTask(items);
				var success = task.Execute();
				Assert.True(success);

				var copied = task.CopiedResources;
				Assert.Equal(items.Length * DpiPath.Ios.Length, copied.Length);

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
	}
}
