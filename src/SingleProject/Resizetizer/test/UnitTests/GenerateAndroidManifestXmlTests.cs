#nullable enable
using System.IO;
using System.Linq;
using System.Xml.Linq;
using Microsoft.Build.Framework;
using Microsoft.Build.Utilities;
using Xunit;

namespace Microsoft.Maui.Resizetizer.Tests
{
	public class GenerateAndroidManifestXmlTests : MSBuildTaskTestFixture<GenerateAndroidManifestXml>
	{
		static readonly XNamespace XmlnsAndroid = "http://schemas.android.com/apk/res/android";

		protected GenerateAndroidManifestXml GetNewTask(
			string manifest,
			ITaskItem? appIcon = null) =>
			new()
			{
				BuildEngine = this,
				Manifest = new TaskItem(manifest),
				IntermediateOutputPath = DestinationDirectory,
				AppIcon = appIcon is null ? null : new[] { appIcon },
			};

		[Theory]
		[InlineData("empty.xml")]
		[InlineData("partial.xml")]
		[InlineData("typical.xml")]
		public void NoFileIsGeneratedWhenThereAreNoImages(string? file)
		{
			var task = GetNewTask($"testdata/androidmanifest/{file}");
			var success = task.Execute();
			Assert.True(success, $"{task.GetType()}.Execute() failed: " + LogErrorEvents.FirstOrDefault()?.Message);

			var msg = Assert.Single(LogMessageEvents);
			Assert.Contains("No changes were made", msg.Message, System.StringComparison.OrdinalIgnoreCase);
			Assert.False(File.Exists(Path.Combine(DestinationDirectory, "AndroidManifest.xml")), "AndroidManifest.xml file was generated.");
		}

		[Fact]
		public void FileIsNotGeneratedWhenAllPropertiesAreSet()
		{
			var appIcon = new TaskItem("images/bicycle.svg");
			appIcon.SetMetadata("IsAppIcon", "true");

			var task = GetNewTask(
				$"testdata/androidmanifest/typical.xml",
				appIcon: appIcon);

			var success = task.Execute();
			Assert.True(success, $"{task.GetType()}.Execute() failed: " + LogErrorEvents.FirstOrDefault()?.Message);

			var msg = Assert.Single(LogMessageEvents);
			Assert.Contains("No changes were made", msg.Message, System.StringComparison.OrdinalIgnoreCase);
			Assert.False(File.Exists(Path.Combine(DestinationDirectory, "AndroidManifest.xml")), "AndroidManifest.xml file was generated.");
		}

		[Theory]
		[InlineData("empty.xml", "@mipmap/bicycle", "@mipmap/bicycle_round")]
		[InlineData("partial.xml", "@mipmap/appicon", "@mipmap/bicycle_round")]
		public void FileIsGeneratedWhenAllPropertiesAreSet(string file, string iconValue, string iconRoundValue)
		{
			var outputPath = Path.Combine(DestinationDirectory, "AndroidManifest.xml");

			var appIcon = new TaskItem("images/bicycle.svg");
			appIcon.SetMetadata("IsAppIcon", "true");

			var task = GetNewTask(
				$"testdata/androidmanifest/{file}",
				appIcon: appIcon);

			var success = task.Execute();
			Assert.True(success, $"{task.GetType()}.Execute() failed: " + LogErrorEvents.FirstOrDefault()?.Message);

			Assert.True(File.Exists(outputPath), "AndroidManifest.xml file was not generated.");

			var xdoc = XDocument.Load(outputPath);

			var xapplication = xdoc.Root!.Element("application")!;
			Assert.Equal(iconValue, xapplication.Attribute(XmlnsAndroid + "icon")!.Value);
			Assert.Equal(iconRoundValue, xapplication.Attribute(XmlnsAndroid + "roundIcon")!.Value);
		}
	}
}
