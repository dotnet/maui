using NUnit.Framework;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.TestCases.Tests.Issues;

public class Issue22469 : _IssuesUITest
{
	const string kClickCount = "Click Count: ";
	const string kClickCountAutomationId = "ClickCount";
	const string kLabelTestAutomationId = "SpanningLabel";

	public Issue22469(TestDevice device)
		: base(device)
	{ }

	public override string Issue => "Crash with specific Font and text in Label";

	[Test]
	[Category(UITestCategories.Label)]
	public void SpanRegionClicking()
	{
		if (Device == TestDevice.Mac)
		{
			Assert.Ignore("Click (x, y) pointer type mouse is not implemented.");
		}

		var label = App.WaitForElement(kLabelTestAutomationId);
		var location = label.GetRect();

		var lineHeight = location.Height / 14;
		var lineCenterOffset = lineHeight / 2;
		var y = location.Y;

		App.TapCoordinates(location.X + 10, y + lineCenterOffset);

		App.TapCoordinates(location.X + 10, y + (lineHeight * 2) + lineCenterOffset);

		App.TapCoordinates(location.X + 10, y + (lineHeight * 13) + lineCenterOffset);

		App.WaitForTextToBePresentInElement(kClickCountAutomationId, $"{kClickCount}{1}");
	}
}