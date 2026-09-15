using NUnit.Framework;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.TestCases.Tests.Issues;

public class Issue33420 : _IssuesUITest
{
	public override string Issue => "System.InvalidCastException when using QueryPropertyAttribute with nullable types";

	public Issue33420(TestDevice device) : base(device) { }

	[Test]
	[Category(UITestCategories.Shell)]
	public void NullableQueryPropertyNavigationShouldNotCrash()
	{
		// Wait for main page to load
		App.WaitForElement("MainLabel");
		
		// Tap navigation button with nullable parameter
		App.Tap("NavigateButton");

		// If the bug is present, the app will crash
		// If fixed, we should navigate successfully to the details page
		App.WaitForElement("ResultLabel");
		
		// Verify the nullable value was passed correctly
		var resultText = App.FindElement("ResultLabel").GetText();
		Assert.That(resultText, Is.EqualTo("Success: ID=1"));
	}
}
