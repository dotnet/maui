using NUnit.Framework;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.TestCases.Tests.Issues;

public class Issue34917 : _IssuesUITest
{
	public Issue34917(TestDevice device) : base(device) { }

	public override string Issue => "[net 11.0][iOS,MacCatalyst] SwipeView.Open() throws ArgumentException on second programmatic call";

	[Test]
	[Category(UITestCategories.SwipeView)]
	public void SwipeViewOpenRightDoesNotThrowOnSecondCall()
	{
		App.WaitForElement(OpenRightButtonId);

		App.Tap(OpenRightButtonId);
		App.WaitForElement(StatusLabelId);
		App.WaitForTextToBePresentInElement(StatusLabelId, "Success");
		Assert.That(App.FindElement(StatusLabelId).GetText(), Is.EqualTo("Success"),
			"First Open(RightItems) call should succeed.");

		App.Tap(OpenRightButtonId);
		App.WaitForTextToBePresentInElement(StatusLabelId, "Success");
		Assert.That(App.FindElement(StatusLabelId).GetText(), Is.EqualTo("Success"),
			"Second consecutive Open(RightItems) call should not throw an exception.");
	}

	[Test]
	[Category(UITestCategories.SwipeView)]
	public void SwipeViewOpenBottomDoesNotThrowOnSecondCall()
	{
		App.WaitForElement(OpenBottomButtonId);
		App.WaitForElement(CloseButtonId);

		App.Tap(CloseButtonId);

		App.Tap(OpenBottomButtonId);
		App.WaitForElement(StatusLabelId);
		App.WaitForTextToBePresentInElement(StatusLabelId, "Success");
		Assert.That(App.FindElement(StatusLabelId).GetText(), Is.EqualTo("Success"),
			"First Open(BottomItems) call should succeed.");

		App.Tap(OpenBottomButtonId);
		App.WaitForTextToBePresentInElement(StatusLabelId, "Success");
		Assert.That(App.FindElement(StatusLabelId).GetText(), Is.EqualTo("Success"),
			"Second consecutive Open(BottomItems) call should not throw an exception.");
	}

	const string OpenRightButtonId = "OpenSwipeButton";
	const string OpenBottomButtonId = "OpenBottomSwipeButton";
	const string CloseButtonId = "CloseSwipeButton";
	const string StatusLabelId = "StatusLabel";
}
