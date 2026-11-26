#if ANDROID // The scenario needs a native Android View.Touch handler
using NUnit.Framework;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.TestCases.Tests.Issues;

public class Issue32226 : _IssuesUITest
{
	public Issue32226(TestDevice device) : base(device) { }

	public override string Issue => "TapGestureRecognizer does not fire when an Android View.Touch handler sets Handled to false";

	[Test]
	[Category(UITestCategories.Gestures)]
	public void TapGestureFiresWhenNativeTouchHandlerDoesNotHandleTouch()
	{
		App.WaitForElement("TapTarget");
		App.Tap("TapTarget");

		Assert.That(App.WaitForElement("NativeTouchLabel").GetText(), Is.EqualTo("Native touch received"));
		Assert.That(App.WaitForElement("ResultLabel").GetText(), Is.EqualTo("Tapped"));
	}

	[Test]
	[Category(UITestCategories.Gestures)]
	public void DoubleTapGestureFiresWhenNativeTouchHandlerDoesNotHandleTouch()
	{
		App.WaitForElement("DoubleTapTarget");
		App.DoubleTap("DoubleTapTarget");

		Assert.That(App.WaitForElement("DoubleTapResultLabel").GetText(), Is.EqualTo("Double tapped"));
	}

	[Test]
	[Category(UITestCategories.Gestures)]
	public void TapGestureFiresWithoutNativeTouchHandler()
	{
		App.WaitForElement("PlainTapTarget");
		App.Tap("PlainTapTarget");

		Assert.That(App.WaitForElement("PlainResultLabel").GetText(), Is.EqualTo("Tapped"));
	}

	[Test]
	[Category(UITestCategories.Gestures)]
	public void TapOnViewDisabledAtRuntimeReachesParent()
	{
		App.WaitForElement("DisableChildButton");
		App.Tap("DisableChildButton");
		App.Tap("ChildTapTarget");

		Assert.That(App.WaitForElement("ParentResultLabel").GetText(), Is.EqualTo("Parent tapped"));
	}

	[Test]
	[Category(UITestCategories.Gestures)]
	public void TapOnViewWithOnlyDropGestureReachesParent()
	{
		App.WaitForElement("DropOnlyChild");
		App.Tap("DropOnlyChild");

		Assert.That(App.WaitForElement("DropParentResultLabel").GetText(), Is.EqualTo("Parent tapped"));
	}
}
#endif
