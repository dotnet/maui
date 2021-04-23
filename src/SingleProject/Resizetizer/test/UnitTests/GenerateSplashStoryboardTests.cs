using System.IO;
using System.Xml.Linq;
using Microsoft.Build.Framework;
using Microsoft.Build.Utilities;
using Xunit;

namespace Microsoft.Maui.Resizetizer.Tests
{
	public class GenerateSplashStoryboardTests : MSBuildTaskTestFixture<GenerateSplashStoryboard>
	{
		readonly string _storyboard;

		public GenerateSplashStoryboardTests()
		{
			_storyboard = Path.Combine(DestinationDirectory, "MauiSplash.storyboard");
		}

		protected GenerateSplashStoryboard GetNewTask(ITaskItem splash) => new()
		{
			OutputFile = _storyboard,
			MauiSplashScreen = new[] { splash },
			BuildEngine = this,
		};

		void AssertFile(string actualPath, params object[] args)
		{
			using var actualStream = File.OpenRead(actualPath);
			var actual = XElement.Load(actualStream);

			using var expectedStream = typeof(GenerateSplashStoryboard).Assembly.GetManifestResourceStream(Path.GetFileName(actualPath));
			using var reader = new StreamReader(expectedStream);
			var expected = XElement.Parse(string.Format(reader.ReadToEnd(), args));
			Assert.True(XNode.DeepEquals(actual, expected), $"{actualPath} did not match:\n{actual}");
		}

		[Theory]
		[InlineData("#abcdef", "0.67058825", "0.8039216", "0.9372549", "1")]
		[InlineData("Green", "0", "0.5019608", "0", "1")]
		public void XmlIsValid(string inputColor, string r, string g, string b, string a)
		{
			var splash = new TaskItem("images/appiconfg.svg");
			splash.SetMetadata("Color", inputColor);
			var task = GetNewTask(splash);

			var success = task.Execute();
			Assert.True(success, $"{task.GetType()}.Execute() failed.");
			AssertFile(_storyboard, "appiconfg.png", r, g, b, a);
		}
	}
}
