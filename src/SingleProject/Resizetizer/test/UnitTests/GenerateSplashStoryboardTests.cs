using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
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

		protected GenerateSplashStoryboard GetNewTask(ITaskItem splash) =>
			new()
			{
				OutputFile = _storyboard,
				MauiSplashScreen = new[] { splash },
				BuildEngine = this,
			};

		void AssertFile(string actualPath, string image, string r, string g, string b, string a)
		{
			using var actualStream = File.OpenRead(actualPath);
			var actual = XElement.Load(actualStream);

			using var expectedBuilder = new StringWriter();
			GenerateSplashStoryboard.SubstituteStoryboard(expectedBuilder, image, r, g, b, a);
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
	}
}
