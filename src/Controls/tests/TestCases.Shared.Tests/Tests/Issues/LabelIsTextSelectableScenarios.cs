using NUnit.Framework;
using NUnit.Framework.Legacy;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.TestCases.Tests.Issues;

public class LabelIsTextSelectableScenarios : _IssuesUITest
{
	static readonly string[] ScenarioIds =
	{
		"Plain", "Padding", "VAlignCenter", "VAlignEnd", "HAlignCenter", "HAlignEnd", "Html", "Formatted",
		"MaxLines", "HeadTrunc", "MiddleTrunc", "Rtl", "Styled", "Disabled", "TapLabel", "LongPressLabel",
	};

	public LabelIsTextSelectableScenarios(TestDevice device) : base(device) { }

	public override string Issue => "Label.IsTextSelectable scenarios keep layout identical";

	// Android cannot ellipsize text that is selectable (its TextView lays out selectable text without truncation support).
	IEnumerable<string> ComparableIds => Device == TestDevice.Android
		? ScenarioIds.Where(id => id is not ("MaxLines" or "HeadTrunc" or "MiddleTrunc"))
		: ScenarioIds;

	Dictionary<string, (double X, double Y, double W, double H)> Measure()
	{
		var result = new Dictionary<string, (double, double, double, double)>();
		foreach (var id in ComparableIds)
		{
			var r = App.WaitForElement(id).GetRect();
			result[id] = (r.X, r.Y, r.Width, r.Height);
		}

		return result;
	}

	// Android reports pixels and lays out selectable text with a different Layout class, which can differ by a pixel or two (less than 1dp).
	void AssertSame(Dictionary<string, (double X, double Y, double W, double H)> a, Dictionary<string, (double X, double Y, double W, double H)> b, string phase)
	{
		foreach (var id in a.Keys)
		{
			var (x1, y1, w1, h1) = a[id];
			var (x2, y2, w2, h2) = b[id];
			var tolerance = Device == TestDevice.Android ? 3 : 1;
			Assert.That(Math.Abs(x1 - x2) <= tolerance && Math.Abs(y1 - y2) <= tolerance && Math.Abs(w1 - w2) <= tolerance && Math.Abs(h1 - h2) <= tolerance,
				$"{phase}: '{id}' moved or resized when IsTextSelectable changed: ({x1},{y1},{w1},{h1}) vs ({x2},{y2},{w2},{h2})");
		}
	}

	// Taps the first glyphs of the label, because the center of a label can be empty space that never reaches the native text view.
	void TapOnText(string id)
	{
		var rect = App.WaitForElement(id).GetRect();
		App.TapCoordinates(rect.X + 20, rect.CenterY());
		ClearSelection();
	}

	// Windows starts a selection with a Copy flyout on the tapped text, which hides the page from automation until it is dismissed.
	void ClearSelection()
	{
		if (Device == TestDevice.Windows)
		{
			App.TapCoordinates(600, 720);
		}
	}

	[Test]
	[Category(UITestCategories.Label)]
	public void TogglingIsTextSelectableDoesNotChangeLayout()
	{
		App.WaitForElement("ToggleAll");
		var on = Measure();

		App.Tap("ToggleAll");
		App.WaitForTextToBePresentInElement("Status", "Selectable: False");
		var off = Measure();
		AssertSame(on, off, "initial");

		App.Tap("ToggleAll");
		App.WaitForTextToBePresentInElement("Status", "Selectable: True");
		AssertSame(on, Measure(), "toggled back on");
	}

	[Test]
	[Category(UITestCategories.Label)]
	public void RuntimeChangesWhileSelectableDoNotChangeLayout()
	{
		App.WaitForElement("Mutate");
		App.Tap("Mutate");
		var on = Measure();

		App.Tap("ToggleAll");
		App.WaitForTextToBePresentInElement("Status", "Selectable: False");
		AssertSame(on, Measure(), "after mutate");
	}

	[Test]
	[Category(UITestCategories.Label)]
	public void LabelTapStillWorksWhenTextIsSelectable()
	{
		TapOnText("TapLabel");
		ClassicAssert.IsTrue(App.WaitForTextToBePresentInElement("GestureResult", "Gesture: label tap"));
	}

	[Test]
	[Category(UITestCategories.Label)]
	public void ShadowedLabelTapStillWorksWhenTextIsSelectable()
	{
		TapOnText("ShadowTapLabel");
		ClassicAssert.IsTrue(App.WaitForTextToBePresentInElement("GestureResult", "Gesture: shadow tap"));
	}

	[Test]
	[Category(UITestCategories.Label)]
	public void LabelTapStillWorksAfterEnablingSelectionAtRuntime()
	{
		TapOnText("LateTapLabel");
		ClassicAssert.IsTrue(App.WaitForTextToBePresentInElement("GestureResult", "Gesture: late tap 1"));

		App.Tap("EnableLate");
		TapOnText("LateTapLabel");
		ClassicAssert.IsTrue(App.WaitForTextToBePresentInElement("GestureResult", "Gesture: late tap 2"));
	}
}
