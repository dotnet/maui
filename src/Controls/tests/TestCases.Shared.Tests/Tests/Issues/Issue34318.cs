using NUnit.Framework;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.TestCases.Tests.Issues;

public class Issue34318 : _IssuesUITest
{
	public Issue34318(TestDevice device) : base(device) { }

	public override string Issue => "Shell Navigating event should fire on ShellContent change";

	[Test]
	[Category(UITestCategories.Shell)]
	public void NavigatingFiresWhenShellContentChanges()
	{
		// Wait for the page and button
		App.WaitForElement("ChangeContentButton");

		// Tap the button to change the current ShellContent
		App.Tap("ChangeContentButton");

		// Wait for label to be present
		App.WaitForElement("ResultLabel");

		// Small delay to allow UI update (common in these tests)
		Thread.Sleep(1000);

		var text = App.FindElement("ResultLabel").GetText();

		Assert.That(text, Is.EqualTo("Navigating"));
	}
}
