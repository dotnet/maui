using NUnit.Framework;
using NUnit.Framework.Legacy;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.TestCases.Tests.Issues;

public class Issue20903 : _IssuesUITest
{
	public Issue20903(TestDevice device) : base(device) { }

	public override string Issue => "Double-tap behavior should work correctly when adding a new double-tap handler";

	//	[Test]
	[Category(UITestCategories.Gestures)]
	public async Task RegisterTwoDoubleTapHandlersAndCheckNumberOfFiredEventsAsync()
	{
		_ = App.WaitForElement("AddDoubleTapHandlerButton");

		App.Tap("AddDoubleTapHandlerButton");
		App.Tap("AddDoubleTapHandlerButton");

		App.DoubleTap("MyLabel");
		await Task.Delay(500);
		App.DoubleTap("MyLabel");

		// Wait for all event handler calls.
		await Task.Delay(1000);

		IUIElement element = App.FindElement("EventCountLabel");
		string? count = element.GetText();
		ClassicAssert.AreEqual("4", count);
	}
}