using NUnit.Framework;
using NUnit.Framework.Legacy;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.TestCases.Tests.Issues;

public class LabelIsTextSelectable : _IssuesUITest
{
	public LabelIsTextSelectable(TestDevice device) : base(device) { }

	public override string Issue => "Label.IsTextSelectable lets the user select text";

	[Test]
	[Category(UITestCategories.Label)]
	public void ToggleIsTextSelectableKeepsLabelVisible()
	{
		App.WaitForElement("SelectableLabel");
		ClassicAssert.AreEqual("IsTextSelectable: True", App.FindElement("StatusLabel").GetText());

		App.Tap("ToggleButton");
		ClassicAssert.AreEqual("IsTextSelectable: False", App.FindElement("StatusLabel").GetText());
		App.WaitForElement("SelectableLabel");

		App.Tap("ToggleButton");
		ClassicAssert.AreEqual("IsTextSelectable: True", App.FindElement("StatusLabel").GetText());
		App.WaitForElement("SelectableLabel");
	}

	[Test]
	[Category(UITestCategories.Label)]
	public void SpanTapStillWorksWhenTextIsSelectable()
	{
		if (Device == TestDevice.Mac)
		{
			Assert.Ignore("Click (x, y) pointer type mouse is not implemented.");
		}

		// A pointer tap on selectable text starts a selection on Windows, and its flyout hides the page from UI automation.
		// The label tap tests in LabelIsTextSelectableScenarios cover tapping selectable text there.
		if (Device == TestDevice.Windows)
		{
			Assert.Ignore("A pointer tap on selectable text opens the selection flyout, which hides the page from UI automation.");
		}

		App.WaitForElement("SelectableSpanLabel");

		var rect = App.WaitForElement("SelectableSpanLabel").GetRect();
		App.TapCoordinates(rect.X + 30, rect.CenterY());

		ClassicAssert.IsTrue(App.WaitForTextToBePresentInElement("TapCountLabel", "Taps: 1"));
	}
}
