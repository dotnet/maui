using NUnit.Framework;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.TestCases.Tests.Issues;

public class Issue36816 : _IssuesUITest
{
	public Issue36816(TestDevice testDevice) : base(testDevice)
	{
	}

	public override string Issue => "Android: Clicks being passed through to controls under other Views";

	[TestCase("ContentView")]
	[TestCase("Grid")]
	[TestCase("StackLayout")]
	[TestCase("BoxView")]
	[TestCase("Border")]
	[Category(UITestCategories.Gestures)]
	public void TapOnOpaqueOverlayShouldNotClickButtonUnderneath(string type)
	{
		SelectOverlay(type);
		App.Tap("OverlayView");
		AssertResult("Not Clicked");

		App.Tap("ToggleTransparency");
		App.Tap("OverlayView");
		AssertResult("Button Clicked");

		App.Tap("ResetResults");
		App.Tap("ToggleTransparency");
		App.Tap("OverlayView");
		AssertResult("Not Clicked");
	}

	[TestCase("ContentView")]
	[TestCase("Grid")]
	[TestCase("StackLayout")]
	[TestCase("BoxView")]
	[TestCase("Border")]
	[Category(UITestCategories.Gestures)]
	public void AncestorTapShouldWorkWithoutClickingUnderlyingButton(string type)
	{
		SelectOverlay(type);
		App.Tap("EnableParentTap");

		var bounds = App.WaitForElement("OverlayView").GetRect();
		App.TapCoordinates(bounds.X + 10, bounds.Y + 10);
		Assert.That(App.FindElement("ParentTapCount").GetText(), Is.EqualTo("1"));
		AssertResult("Not Clicked");

		if (type != "BoxView")
		{
			App.Tap("CardLabel");
			Assert.That(App.FindElement("ParentTapCount").GetText(), Is.EqualTo("2"));
			AssertResult("Not Clicked");
		}
	}

	[TestCase("ContentView")]
	[TestCase("Grid")]
	[Category(UITestCategories.Gestures)]
	public void AncestorPanShouldReceiveCompleteSequence(string type)
	{
		SelectOverlay(type);
		App.Tap("EnableParentPan");
		var bounds = App.WaitForElement("OverlayView").GetRect();
		var y = bounds.Y + bounds.Height / 2;
		App.DragCoordinates(bounds.X + 20, y, bounds.X + bounds.Width - 20, y);

		Assert.That(App.FindElement("ParentPanStatus").GetText(), Is.EqualTo("Completed"));
		AssertResult("Not Clicked");
	}

	[TestCase("ContentView", "Shadow")]
	[TestCase("ContentView", "Clip")]
	[TestCase("Grid", "Shadow")]
	[TestCase("Grid", "Clip")]
	[Category(UITestCategories.Gestures)]
	public void AncestorTapShouldWorkWhenWrapperIsAddedAndRemoved(string type, string effect)
	{
		SelectOverlay(type);
		App.Tap("EnableParentTap");
		App.Tap("Toggle" + effect);
		App.Tap("CardLabel");
		Assert.That(App.FindElement("ParentTapCount").GetText(), Is.EqualTo("1"));
		AssertResult("Not Clicked");

		App.Tap("Toggle" + effect);
		App.Tap("CardLabel");
		Assert.That(App.FindElement("ParentTapCount").GetText(), Is.EqualTo("2"));
		AssertResult("Not Clicked");
	}

	[TestCase("ContentView", "Shadow")]
	[TestCase("ContentView", "Clip")]
	[TestCase("Grid", "Shadow")]
	[TestCase("Grid", "Clip")]
	[Category(UITestCategories.Gestures)]
	public void WrappedOverlayShouldRespectTransparencyChanges(string type, string effect)
	{
		SelectOverlay(type);
		App.Tap("Toggle" + effect);
		App.Tap("OverlayView");
		AssertResult("Not Clicked");

		App.Tap("ToggleTransparency");
		App.Tap("OverlayView");
		AssertResult("Button Clicked");

		App.Tap("ResetResults");
		App.Tap("ToggleTransparency");
		App.Tap("OverlayView");
		AssertResult("Not Clicked");
	}

	[TestCase("ContentView")]
	[TestCase("Grid")]
	[TestCase("Border")]
	[Category(UITestCategories.Gestures)]
	public void InteractiveChildShouldKeepNativeTouchHandling(string type)
	{
		SelectOverlay(type);
		App.Tap("EnableParentTap");
		App.Tap("ShowChildButton");
		App.Tap("ChildButton");

		Assert.That(App.FindElement("ChildResultLabel").GetText(), Is.EqualTo("Button Clicked"));
		Assert.That(App.FindElement("ParentTapCount").GetText(), Is.EqualTo("0"));
		AssertResult("Not Clicked");
	}

	[TestCase("Shadow")]
	[TestCase("Clip")]
	[Category(UITestCategories.Gestures)]
	public void TransparentWrappedLayoutShouldKeepNonTransparentChildrenInteractive(string effect)
	{
		SelectOverlay("Grid");
		App.Tap("ShowChildButton");
		App.Tap("ToggleCascade");
		App.Tap("ToggleTransparency");
		App.Tap("Toggle" + effect);
		App.Tap("ChildButton");

		Assert.That(App.FindElement("ChildResultLabel").GetText(), Is.EqualTo("Button Clicked"));
		AssertResult("Not Clicked");
	}

	void SelectOverlay(string type)
	{
		App.WaitForElement("Select" + type);
		App.Tap("Select" + type);
		App.WaitForElement("OverlayView");
	}

	void AssertResult(string expected) =>
		Assert.That(App.FindElement("ResultLabel").GetText(), Is.EqualTo(expected));
}
