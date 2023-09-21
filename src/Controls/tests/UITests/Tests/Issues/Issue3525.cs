using Microsoft.Maui.Appium;
using NUnit.Framework;

namespace Microsoft.Maui.AppiumTests.Issues;

public class Issue3525 : _IssuesUITest
{
	const string kClickCount = "Click Count: ";
	const string kClickCountAutomationId = "ClickCount";
	const string kLabelTestAutomationId = "SpanningLabel";

	public Issue3525(TestDevice device)
		: base(device)
	{ }

	public override string Issue => "Finicky tap gesture recognition on Spans";

	[Test]
	public void SpanRegionClicking()
	{
		// This issue is not working on net7 for the following platforms 
		// This is not a regression it's just the test being backported from net8
		UITestContext.IgnoreIfPlatforms(new[]
		{
				TestDevice.Android, TestDevice.Windows, TestDevice.iOS, TestDevice.Mac
		}, BackportedTestMessage);

		if (Device == TestDevice.Mac ||
			Device == TestDevice.iOS ||
			Device == TestDevice.Windows)
		{
			Assert.Ignore("This test is failing on iOS/Mac Catalyst/Windows because the feature is not yet implemented: https://github.com/dotnet/maui/issues/4734");
		}

		var label = App.WaitForElement(kLabelTestAutomationId);
		var location = label[0].Rect;

		var lineHeight = location.Height / 5;
		var lineCenterOffset = lineHeight / 2;
		var y = location.Y;

		App.TapCoordinates(location.X + 10, y + lineCenterOffset);

		App.TapCoordinates(location.X + 10, y + (lineHeight * 2) + lineCenterOffset);

		App.TapCoordinates(location.X + 10, y + (lineHeight * 4) + lineCenterOffset);

		App.WaitForTextToBePresentInElement(kClickCountAutomationId, $"{kClickCount}{1}");
	}
}
