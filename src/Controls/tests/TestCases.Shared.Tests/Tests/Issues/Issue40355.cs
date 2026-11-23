using NUnit.Framework;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.TestCases.Tests.Issues;

public class Issue40355 : _IssuesUITest
{
	public Issue40355(TestDevice device) : base(device) { }

	public override string Issue => "LongPressGestureRecognizer Command does not work on Button";

	// TouchAndHold is not supported on Mac Catalyst
	void PerformLongPress(string elementId)
	{
		App.WaitForElement(elementId);
#if MACCATALYST
		App.LongPress(elementId);
#else
		App.TouchAndHold(elementId);
#endif
	}

	[Test]
	[Category(UITestCategories.Gestures)]
	public void LongPressGestureCommandFiresOnButton()
	{
		PerformLongPress("Issue40355Button");

		var status = App.WaitForElement("Issue40355StatusLabel").GetText();
		Assert.That(status, Is.EqualTo("Long press detected"));
	}

	[Test]
	[Category(UITestCategories.Gestures)]
	public void ButtonClickStillFiresWithLongPressGesture()
	{
		App.WaitForElement("Issue40355Button");
		App.Tap("Issue40355Button");

		var status = App.WaitForElement("Issue40355ClickLabel").GetText();
		Assert.That(status, Is.EqualTo("Clicked"));
	}

	[Test]
	[Category(UITestCategories.Gestures)]
	public void LongPressGestureCommandFiresOnImageButton()
	{
		PerformLongPress("Issue40355ImageButton");

		var status = App.WaitForElement("Issue40355ImageButtonStatusLabel").GetText();
		Assert.That(status, Is.EqualTo("ImageButton long press detected"));
	}
}
