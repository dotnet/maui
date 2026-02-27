using NUnit.Framework;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.TestCases.Tests.Issues;

public class Issue34272 : _IssuesUITest
{
	public override string Issue => "ObjectDisposed Exception Closing an App on Windows with style triggers";

	public Issue34272(TestDevice device) : base(device) { }

	// The root bug: ObjectDisposedException is thrown when the app closes on Windows if
	// style triggers are present. This test verifies style triggers apply/un-apply correctly
	// and the UI remains functional through trigger state changes (exercising the trigger
	// teardown code path that is involved in the crash).
	[Test]
	[Category(UITestCategories.LifeCycle)]
	public void StyleTriggerAppliesAndRemovesWithoutException()
	{
		// Wait for the page to load
		App.WaitForElement("TriggerButton");
		App.WaitForElement("ToggleButton");
		App.WaitForElement("StatusLabel");

		// Initial state: trigger should NOT be fired — button text is "Enabled"
		var initialText = App.FindElement("TriggerButton").GetText();
		Assert.That(initialText, Is.EqualTo("Enabled"),
			"Button should start in the Enabled state before the trigger fires.");

		// Fire the trigger: disable the button → Trigger.Value=false should apply setters
		App.Tap("ToggleButton");

		var statusAfterDisable = App.FindElement("StatusLabel").GetText();
		Assert.That(statusAfterDisable, Is.EqualTo("Trigger: Fired"),
			"Status label should reflect that the trigger has fired.");

		var textAfterDisable = App.FindElement("TriggerButton").GetText();
		Assert.That(textAfterDisable, Is.EqualTo("Disabled"),
			"Style trigger should have set Button.Text to 'Disabled' when IsEnabled=false.");

		// Un-fire the trigger: re-enable the button → setters should be removed
		App.Tap("ToggleButton");

		var statusAfterEnable = App.FindElement("StatusLabel").GetText();
		Assert.That(statusAfterEnable, Is.EqualTo("Trigger: Not Fired"),
			"Status label should show trigger is no longer fired.");

		var textAfterEnable = App.FindElement("TriggerButton").GetText();
		Assert.That(textAfterEnable, Is.EqualTo("Enabled"),
			"Style trigger should restore Button.Text to 'Enabled' when IsEnabled=true.");
	}
}
