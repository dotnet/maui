using NUnit.Framework;
using NUnit.Framework.Legacy;
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
			const string automationId = "StartButton";

			var button = App.WaitForElement(automationId);
			App.Tap(automationId);

			var timeout = TimeSpan.FromMinutes(5);  // MACCATALYST takes a long time to run this test
			App.WaitForTextToBePresentInElement(automationId, ",", timeout);

			var times = button.GetText()?.Split(',') ?? throw new ArgumentNullException("StartButton text is null");

			var logMessage = @$"RenderingPerformance: [{times[0]}, {times[1]}, {times[2]}]";
			TestContext.WriteLine(logMessage);

			// Write the log to a file and attach it to the test results for ADO
			var logFile = Path.Combine(Path.GetTempPath(), "RenderingPerformance.log");
			await File.WriteAllTextAsync(logFile, logMessage);
			TestContext.AddTestAttachment(logFile, "RenderingPerformance.log");
		}
	}
}
