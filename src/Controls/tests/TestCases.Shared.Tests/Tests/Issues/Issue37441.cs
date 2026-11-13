using NUnit.Framework;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.TestCases.Tests.Issues;

public class Issue37441 : _IssuesUITest
{
	public Issue37441(TestDevice device) : base(device) { }

	public override string Issue => "Concurrent bindable property updates and off-thread visual state changes";

	[Test]
	[Category(UITestCategories.VisualStateManager)]
	public void ConcurrentBindablePropertyUpdatesCompleteWithoutFailure()
	{
		RunScenario("ScenarioAButton", "ScenarioAStatus");
	}

	[Test]
	[Category(UITestCategories.VisualStateManager)]
	public void OffThreadVisualStateChangesCompleteWithoutFailure()
	{
		RunScenario("ScenarioBButton", "ScenarioBStatus");
	}

	void RunScenario(string buttonAutomationId, string statusAutomationId)
	{
		App.WaitForElement(buttonAutomationId);
		App.Tap(buttonAutomationId);

		Assert.That(
			App.WaitForTextToBePresentInElement(statusAutomationId, "PASS", timeout: TimeSpan.FromSeconds(20)),
			Is.True,
			$"{statusAutomationId} did not report a clean completion.");

		var status = App.FindElement(statusAutomationId).GetText();
		Assert.That(status, Is.EqualTo("PASS"), $"Unexpected scenario result: {status}");
	}
}
