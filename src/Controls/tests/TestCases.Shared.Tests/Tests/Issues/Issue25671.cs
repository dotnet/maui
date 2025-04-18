#if ANDROID || IOS //This test is only valid for Android and iOS as it measures the layout passes.
using System.Text;
using System.Text.RegularExpressions;
using NUnit.Framework;
using NUnit.Framework.Legacy;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.TestCases.Tests.Issues
{
	public class Issue25671 : _IssuesUITest
	{
		public Issue25671(TestDevice testDevice) : base(testDevice)
		{
		}

		public override string Issue => "Layout passes should not increase";

		[Test]
		[Category(UITestCategories.CollectionView)]
		public async Task LayoutPassesShouldNotIncrease()
		{
			App.WaitForElement("RegenerateItems");

			await ScrollDown();
			await ScrollDown();
			await ScrollDown();
			await ScrollUp();
			await ScrollUp();

			App.Tap("RegenerateItems");

			await ScrollDown();
			await ScrollDown();
			await ScrollUp();
			await ScrollUp();

			App.Tap("PressMe");
			await Task.Delay(100);

			// Text will be in the format "M: 0, A: 0"
			var text = App.WaitForElement("PressMe").GetText()!;
			// Get the stats text
			var statsLabel = App.WaitForElement("Stats");
			var statsBuilder = new StringBuilder();
			while (true)
			{
				try
				{
					var line = statsLabel.GetText();
					if (string.IsNullOrEmpty(line))
					{
						break;
					}

					statsBuilder.AppendLine(line);
					statsLabel.Tap();
					await Task.Delay(10);
				}
				catch
				{
					break;
				}
			}

			// Get standalone measure passes and arrange passes counts via regex
			var match = Regex.Match(text, @"M: (\d+), A: (\d+), CPM: (\d+), CPA: (\d+)");
			var measurePasses = int.Parse(match.Groups[1].Value);
			var arrangePasses = int.Parse(match.Groups[2].Value);
			var crossPlatformMeasurePasses = int.Parse(match.Groups[3].Value);
			var crossPlatformArrangePasses = int.Parse(match.Groups[4].Value);

#if IOS
            var maxMeasurePasses = 193;
            var maxArrangePasses = 247;
            var maxCrossPlatformMeasurePasses = 75;
            var maxCrossPlatformArrangePasses = 130;

            if (App.FindElement("HeadingLabel").GetText() == "CollectionViewHandler2")
            {
	            maxMeasurePasses = 257;
	            maxArrangePasses = 235;
	            maxCrossPlatformMeasurePasses = 103;
	            maxCrossPlatformArrangePasses = 132;
            }
#elif ANDROID
			const int maxMeasurePasses = 206;
			const int maxArrangePasses = 215;
			const int maxCrossPlatformMeasurePasses = 66;
			const int maxCrossPlatformArrangePasses = 7;
#endif

			var logMessage = @$"Measure passes: {measurePasses}, Arrange passes: {arrangePasses}";
			logMessage += $"\nCrossPlatformMeasure passes: {crossPlatformMeasurePasses}, CrossPlatformArrange passes: {crossPlatformArrangePasses}";
			logMessage += $"\n\n{statsBuilder}";
			TestContext.WriteLine(logMessage);

			// Write the log to a file and attach it to the test results for ADO
			var logFile = Path.Combine(Path.GetTempPath(), "LayoutPasses.log");
			File.WriteAllText(logFile, logMessage);
			TestContext.AddTestAttachment(logFile, "LayoutPasses.log");

			// Then assert that the measure passes and arrange passes are less than the expected values.
			// Let's give a 5% margin of error.
			ClassicAssert.LessOrEqual(measurePasses, maxMeasurePasses * 1.05);
			ClassicAssert.LessOrEqual(arrangePasses, maxArrangePasses * 1.05);
			ClassicAssert.LessOrEqual(crossPlatformMeasurePasses, maxCrossPlatformMeasurePasses * 1.05);
			ClassicAssert.LessOrEqual(crossPlatformArrangePasses, maxCrossPlatformArrangePasses * 1.05);
		}

		async Task ScrollDown()
		{
			await Task.Delay(50);
			App.ScrollDown("CV", swipePercentage: 0.5D, swipeSpeed: 1000);
			await Task.Delay(50);
		}

		async Task ScrollUp()
		{
			await Task.Delay(50);
			App.ScrollUp("CV", swipePercentage: 0.5D, swipeSpeed: 1000);
			await Task.Delay(50);
		}
	}
}
#endif