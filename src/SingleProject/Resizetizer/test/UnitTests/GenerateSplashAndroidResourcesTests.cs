using System.IO;
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

		protected GenerateSplashAndroidResources GetNewTask(ITaskItem splash) => new()
		{
			MauiSplashScreen = new[] { splash },
			ColorsFile = _colors,
			DrawableFile = _drawable,
			BuildEngine = this,
		};

		void AssertFile(string actualPath, params object[] args)
		{
			using var actualStream = File.OpenRead(actualPath);
			var actual = XElement.Load(actualStream);

			using var expectedStream = GetType().Assembly.GetManifestResourceStream(Path.GetFileName(actualPath));
			using var reader = new StreamReader(expectedStream);
			var expected = XElement.Parse(string.Format(reader.ReadToEnd(), args));
			Assert.True(XNode.DeepEquals(actual, expected), $"{actualPath} did not match:\n{actual}");
		}

		[Theory]
		[InlineData("#abcdef", "#ffabcdef")]
		[InlineData("Red", "#ffff0000")]
		public void XmlIsValid(string inputColor, string outputColor)
		{
			var splash = new TaskItem("images/appiconfg.svg");
			splash.SetMetadata("Color", inputColor);
			var task = GetNewTask(splash);

			var success = task.Execute();
			Assert.True(success, $"{task.GetType()}.Execute() failed.");
			AssertFile(_colors, outputColor);
			AssertFile(_drawable, "@drawable/appiconfg");
		}
	}
}
