using NUnit.Framework;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.TestCases.Tests.Issues;

public class Issue36298 : _IssuesUITest
{
	public Issue36298(TestDevice device) : base(device) { }

	public override string Issue => "[Windows] ContentPresenter throws ArgumentException when dynamically switching RefreshView or ScrollView content.";

	[Test]
	[Category(UITestCategories.Layout)]
	public void SwitchingContentPresenterContentShouldNotCrash()
	{
		// Wait for the page to load showing View 1
		App.WaitForElement("SwitchToView2");

		// Switch to View 2
		App.Tap("SwitchToView2");
		App.WaitForElement("SwitchToView1");

		// Switch back to View 1 — this triggered the ArgumentException before the fix
		App.Tap("SwitchToView1");

		// Label is updated to "Success" only if the switch completed without crashing.
		// WaitForElement("SuccessLabel") alone is insufficient because the label
		// is present from page load; we must verify the text was actually updated.
		Assert.That(App.WaitForTextToBePresentInElement("SuccessLabel", "Success"), Is.True);
	}
}
