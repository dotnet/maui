using NUnit.Framework;
using UITest.Appium;
using UITest.Core;

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
		if (Device == TestDevice.Mac ||
			Device == TestDevice.iOS ||
			Device == TestDevice.Windows)
		{
			Assert.Ignore("This test is failing on iOS/Mac Catalyst/Windows because the feature is not yet implemented: https://github.com/dotnet/maui/issues/4734");
		}

		var label = App.WaitForElement(kLabelTestAutomationId);
		var location = label.GetRect();

		var lineHeight = location.Height / 5;
		var lineCenterOffset = lineHeight / 2;
		var y = location.Y;

		App.Click(location.X + 10, y + lineCenterOffset);

		App.Click(location.X + 10, y + (lineHeight * 2) + lineCenterOffset);

		App.Click(location.X + 10, y + (lineHeight * 4) + lineCenterOffset);

		App.WaitForTextToBePresentInElement(kClickCountAutomationId, $"{kClickCount}{1}");
	}
}
