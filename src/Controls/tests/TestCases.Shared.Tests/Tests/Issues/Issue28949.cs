using NUnit.Framework;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.TestCases.Tests.Issues;

public class Issue28949 : _IssuesUITest
{
	public Issue28949(TestDevice device) : base(device)
	{
	}

	public override string Issue => "On iOS GestureRecognizers don't work on Span in a Label, which doesn't get IsVisible (=true) update from its parent";

	[Test]
	[Category(UITestCategories.Label)]
	public void GestureRecognizersOnLabelSpanShouldWork()
	{
		App.WaitForElement("ToggleButton");
		App.Tap("ToggleButton");
		var spanRect = App.WaitForElement("Label").GetRect();
		App.TapCoordinates(spanRect.X + 5, spanRect.Y + 5);
		App.WaitForElement("Success");
	}
}