using NUnit.Framework;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.TestCases.Tests.Issues;

public class Issue35399 : _IssuesUITest
{
	public Issue35399(TestDevice device) : base(device) { }

	public override string Issue => "VisualElement.ChangeVisualState() gets stuck in Selected state";

	[Test]
	[Category(UITestCategories.VisualStateManager)]
	public void ChangeVisualStateShouldExitSelectedStateAfterDeselect()
	{
		App.WaitForElement(SelectButtonId);

		// Transition to Selected state
		App.Tap(SelectButtonId);
		var stateAfterSelect = App.FindElement(StateLabelId).GetText();
		Assert.That(stateAfterSelect, Is.EqualTo("Selected"),
			"After selecting, the element should be in the Selected visual state.");

		// Deselect: base.ChangeVisualState() should transition back to Normal.
		// Regression: on .NET 10, IsElementInSelectedState() still reads "Selected" from
		// the VSM group's CurrentState, causing base to re-apply Selected even though
		// IsSelected is now false.
		App.Tap(DeselectButtonId);
		var stateAfterDeselect = App.FindElement(StateLabelId).GetText();
		Assert.That(stateAfterDeselect, Is.EqualTo("Normal"),
			"After deselecting, base.ChangeVisualState() must transition the element to Normal, not remain stuck in Selected.");
	}

	const string SelectButtonId = "SelectButton";
	const string DeselectButtonId = "DeselectButton";
	const string StateLabelId = "StateLabel";
}
