using System.Drawing;
using NUnit.Framework;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.AppiumTests.Issues;

public class Issue20903 : _IssuesUITest
{
	public Issue20903(TestDevice device) : base(device) { }

	public override string Issue => "Double-tap behavior should work correctly when adding a new double-tap handler";

	[Test]
	public async Task RegisterTwoDoubleTapHandlersAndCheckNumberOfFiredEventsAsync()
	{
		_ = App.WaitForElement("AddDoubleTapHandlerButton");

		App.Click("AddDoubleTapHandlerButton");
		App.Click("AddDoubleTapHandlerButton");

		App.DoubleClick("MyLabel");
		await Task.Delay(500);
		App.DoubleClick("MyLabel");

		// Wait for all event handler calls.
		await Task.Delay(1000);

		IUIElement element = App.FindElement("EventCountLabel");
		string? count = element.GetText();
		Assert.AreEqual("4", count);
	}
}