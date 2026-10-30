using System.Collections.Generic;
using System.IO;
using System.Linq;
using Microsoft.Build.Framework;
using Microsoft.Build.Utilities;
using Xunit;
using Xunit.Abstractions;

namespace Microsoft.Maui.Resizetizer.Tests
{
	/// <summary>Linux / GTK 4 (platform type "gtk"): image and icon processing.</summary>
	public class ExecuteForGtk : ResizetizeImagesTests.ExecuteForPlatformApp
	{
		public ExecuteForGtk(ITestOutputHelper output)
			: base(output)
		{
		}

		protected override string Platform => "gtk";

		protected override string GetPlatformOutputFileName(string file) =>
			$"Resources/Images/{file}";

		[Fact]
		public void NoItemsSucceed()
		{
			var task = GetNewTask();

			Assert.True(task.Execute(), LogErrorEvents.FirstOrDefault()?.Message);
		}

		[Fact]
		public void VectorImageIsRasterizedAtTheBaseSizeInASingleScale()
		{
			var items = new[]
			{
				new TaskItem("images/camera.svg", new Dictionary<string, string> { ["BaseSize"] = "64,64" }),
			};

			var task = GetNewTask(items);
			Assert.True(task.Execute(), LogErrorEvents.FirstOrDefault()?.Message);

			AssertFileSize("Resources/Images/camera.png", 64, 64);
			Assert.Single(task.CopiedResources);
		}

		[Fact]
		public void CopiedResourceCarriesTheDpiPath()
		{
			var items = new[]
			{
				new TaskItem("images/camera.svg", new Dictionary<string, string> { ["BaseSize"] = "64,64" }),
			};

			var task = GetNewTask(items);
			Assert.True(task.Execute(), LogErrorEvents.FirstOrDefault()?.Message);

			var resource = GetCopiedResource(task, "Resources/Images/camera.png");
			Assert.Equal("Resources/Images", resource.GetMetadata("_ResizetizerDpiPath"));
			Assert.Equal("1.0", resource.GetMetadata("_ResizetizerDpiScale"));
		}

		[Theory]
		[InlineData("appicon", null, "appicon")]
		[InlineData("appicon", "the_alias", "the_alias")]
		[InlineData("appicon", "folder/the_alias.png", "the_alias")]
		public void VectorAppIconGeneratesHicolorPngs(string name, string alias, string outputName)
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
			Assert.True(task.Execute(), LogErrorEvents.FirstOrDefault()?.Message);

			AssertFileSize($"Resources/Icons/hicolor/48x48/apps/{outputName}.png", 48, 48);
			AssertFileSize($"Resources/Icons/hicolor/128x128/apps/{outputName}.png", 128, 128);
			AssertFileSize($"Resources/Icons/hicolor/256x256/apps/{outputName}.png", 256, 256);
			Assert.Equal(3, task.CopiedResources.Length);
		}

		[Fact]
		public void RasterAppIconGeneratesHicolorPngs()
		{
			var items = new[]
			{
				new TaskItem("images/camera_color.png", new Dictionary<string, string>
				{
					["IsAppIcon"] = bool.TrueString,
					["Link"] = "icon",
				}),
			};

			var task = GetNewTask(items);
			Assert.True(task.Execute(), LogErrorEvents.FirstOrDefault()?.Message);

			AssertFileSize("Resources/Icons/hicolor/48x48/apps/icon.png", 48, 48);
			AssertFileSize("Resources/Icons/hicolor/256x256/apps/icon.png", 256, 256);
		}

		[Fact]
		public void ImagesAndIconTogether()
		{
			var items = new[]
			{
				new TaskItem("images/camera.svg", new Dictionary<string, string> { ["BaseSize"] = "32,32" }),
				new TaskItem("images/appicon.svg", new Dictionary<string, string> { ["IsAppIcon"] = bool.TrueString }),
			};

			var task = GetNewTask(items);
			Assert.True(task.Execute(), LogErrorEvents.FirstOrDefault()?.Message);

			AssertFileExists("Resources/Images/camera.png");
			AssertFileExists("Resources/Icons/hicolor/128x128/apps/appicon.png");
			AssertFileNotExists("Resources/Images/appicon.png");
		}
	}

	public class GtkDpiPathTests
	{
		[Fact]
		public void OriginalIsTheImagesFolderAtScaleOne()
		{
			var original = DpiPath.GetOriginal("gtk");

			Assert.Equal("Resources/Images", original.Path);
			Assert.Equal(1.0m, original.Scale);
		}

		[Fact]
		public void ImagesAreASingleScale()
		{
			var dpis = DpiPath.GetDpis("GTK");

			var dpi = Assert.Single(dpis);
			Assert.Equal("Resources/Images", dpi.Path);
			Assert.Equal(string.Empty, dpi.FileSuffix);
		}

		[Fact]
		public void AppIconFollowsTheIconThemeLayout()
		{
			var dpis = DpiPath.GetAppIconDpis("gtk", "appicon");

			Assert.Equal(
				new[] { "Resources/Icons/hicolor/48x48/apps", "Resources/Icons/hicolor/128x128/apps", "Resources/Icons/hicolor/256x256/apps" },
				dpis.Select(d => d.Path).ToArray());
			Assert.Equal(new[] { 48f, 128f, 256f }, dpis.Select(d => d.Size!.Value.Width).ToArray());
		}
	}

	public class GenerateGtkDesktopFileTests : MSBuildTaskTestFixture<GenerateGtkDesktopFile>
	{
		public GenerateGtkDesktopFileTests(ITestOutputHelper output)
			: base(output)
		{
		}

		GenerateGtkDesktopFile NewTask(string applicationId = "com.contoso.notes", params ITaskItem[] icons) =>
			new GenerateGtkDesktopFile
			{
				BuildEngine = this,
				IntermediateOutputPath = DestinationDirectory,
				ApplicationId = applicationId,
				ApplicationTitle = "Contoso Notes",
				ApplicationDisplayVersion = "1.2",
				Executable = "ContosoNotes",
				AppIcon = icons,
			};

		[Fact]
		public void WritesTheEntryNamedAfterTheApplicationId()
		{
			var task = NewTask(icons: new ITaskItem[] { new TaskItem("images/appicon.svg", new Dictionary<string, string> { ["IsAppIcon"] = "True" }) });

			Assert.True(task.Execute(), LogErrorEvents.FirstOrDefault()?.Message);

			Assert.Equal(Path.Combine(DestinationDirectory, "com.contoso.notes.desktop"), task.GeneratedDesktopFile!.ItemSpec);
			var text = File.ReadAllText(task.GeneratedDesktopFile.ItemSpec);
			Assert.StartsWith("[Desktop Entry]\n", text, System.StringComparison.Ordinal);
			Assert.Contains("Type=Application\n", text, System.StringComparison.Ordinal);
			Assert.Contains("Name=Contoso Notes\n", text, System.StringComparison.Ordinal);
			Assert.Contains("Exec=ContosoNotes\n", text, System.StringComparison.Ordinal);
			Assert.Contains("Icon=appicon\n", text, System.StringComparison.Ordinal);
			Assert.Contains("StartupWMClass=com.contoso.notes\n", text, System.StringComparison.Ordinal);
			Assert.Contains("Categories=Utility;\n", text, System.StringComparison.Ordinal);
			Assert.Contains("X-AppVersion=1.2\n", text, System.StringComparison.Ordinal);
			Assert.DoesNotContain("\r", text, System.StringComparison.Ordinal);
		}

		[Fact]
		public void IconNameIsTheAliasWithoutExtension()
		{
			var task = NewTask(icons: new ITaskItem[] { new TaskItem("images/appicon.svg", new Dictionary<string, string> { ["Link"] = "Folder/the_icon.png" }) });

			Assert.True(task.Execute(), LogErrorEvents.FirstOrDefault()?.Message);

			Assert.Contains("Icon=the_icon\n", File.ReadAllText(task.GeneratedDesktopFile!.ItemSpec), System.StringComparison.Ordinal);
		}

		[Fact]
		public void NoIconNoIconLine()
		{
			var task = NewTask();

			Assert.True(task.Execute(), LogErrorEvents.FirstOrDefault()?.Message);

			Assert.DoesNotContain("Icon=", File.ReadAllText(task.GeneratedDesktopFile!.ItemSpec), System.StringComparison.Ordinal);
		}

		[Fact]
		public void NoApplicationIdGeneratesNothing()
		{
			var task = NewTask(applicationId: "");

			Assert.True(task.Execute(), LogErrorEvents.FirstOrDefault()?.Message);

			Assert.Null(task.GeneratedDesktopFile);
			Assert.False(Directory.Exists(DestinationDirectory) && Directory.GetFiles(DestinationDirectory).Any());
		}

		[Fact]
		public void MissingTitleUsesTheExecutable()
		{
			var task = NewTask();
			task.ApplicationTitle = null;

			Assert.True(task.Execute(), LogErrorEvents.FirstOrDefault()?.Message);

			Assert.Contains("Name=ContosoNotes\n", File.ReadAllText(task.GeneratedDesktopFile!.ItemSpec), System.StringComparison.Ordinal);
		}

		[Fact]
		public void UnchangedContentIsNotRewritten()
		{
			var task = NewTask();
			Assert.True(task.Execute(), LogErrorEvents.FirstOrDefault()?.Message);
			var path = task.GeneratedDesktopFile!.ItemSpec;
			var stamp = new System.DateTime(2001, 1, 1, 0, 0, 0, System.DateTimeKind.Utc);
			File.SetLastWriteTimeUtc(path, stamp);

			task = NewTask();
			Assert.True(task.Execute(), LogErrorEvents.FirstOrDefault()?.Message);

			Assert.Equal(stamp, File.GetLastWriteTimeUtc(path));
		}

		[Fact]
		public void CustomCategoriesAreTerminated()
		{
			var task = NewTask();
			task.Categories = "Office;Utility";

			Assert.True(task.Execute(), LogErrorEvents.FirstOrDefault()?.Message);

			Assert.Contains("Categories=Office;Utility;\n", File.ReadAllText(task.GeneratedDesktopFile!.ItemSpec), System.StringComparison.Ordinal);
		}

		[Theory]
		[InlineData("My App", "\"My App\"")]
		[InlineData("app$x", "\"app\\\\$x\"")]
		[InlineData("plain", "plain")]
		public void ExecIsQuotedAndEscaped(string executable, string expected)
		{
			var text = GenerateGtkDesktopFile.Build("a.b", "Title", null, executable, null, null);

			Assert.Contains($"Exec={expected}\n", text, System.StringComparison.Ordinal);
		}

		[Fact]
		public void ValuesAreEscaped()
		{
			var text = GenerateGtkDesktopFile.Build("a.b", "Line1\nLine2\\", null, "app", null, null);

			Assert.Contains("Name=Line1\\nLine2\\\\\n", text, System.StringComparison.Ordinal);
		}
	}
}
