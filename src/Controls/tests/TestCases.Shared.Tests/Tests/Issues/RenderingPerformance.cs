using NUnit.Framework;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.TestCases.Tests.Issues
{
	public class RenderingPerformance : _IssuesUITest
	{
		public RenderingPerformance(TestDevice device) : base(device) { }

		public override string Issue => "Rendering performance";

		[Test]
		[Category(UITestCategories.Performance)]
		public async Task RenderingPerformanceRun()
		{
			var button = App.WaitForElement("StartButton");
			App.Tap("StartButton");

			string? text;
			while (true)
			{
				text = button.GetText();
				if (string.IsNullOrEmpty(text) || text == "Start")
				{
					await Task.Delay(1000);
					continue;
				}

				break;
			}

			var times = text.Split(',');

			TestContext.WriteLine(@$"RenderingPerformance: [{times[0]}, {times[1]}, {times[2]}]");
		}
	}
}
