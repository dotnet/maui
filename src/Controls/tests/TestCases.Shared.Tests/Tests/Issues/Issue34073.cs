using NUnit.Framework;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.TestCases.Tests.Issues;

public class Issue34073 : _IssuesUITest
{
	public override string Issue => "OnNavigatingFrom is reporting wrong DestinationPage";

	public Issue34073(TestDevice device) : base(device) { }

	[Test]
	[Category(UITestCategories.Shell)]
	public void OnNavigatingFromShouldReportCorrectDestinationPage()
	{
		// Wait for Page A to load
		App.WaitForElement("NavigateButton");

		// Navigate A → B, which triggers OnNavigatingFrom on PageA
		// The bug: DestinationPage is PageA (self) instead of PageB
		App.Tap("NavigateButton");

		// Wait for Page B to appear
		App.WaitForElement("GoBackButton");

		// Navigate back to Page A so we can read the captured result label
		App.Tap("GoBackButton");

		// Wait for Page A to reappear
		App.WaitForElement("NavigatingFromResult");

		// DestinationPage should be Issue34073PageB (the page we navigated TO)
		// Bug causes it to report Issue34073PageA (the current/source page)
		var result = App.FindElement("NavigatingFromResult").GetText();
		Assert.That(result, Is.EqualTo("Issue34073PageB"),
			$"OnNavigatingFrom.DestinationPage should be PageB but was: {result}");
	}
}
