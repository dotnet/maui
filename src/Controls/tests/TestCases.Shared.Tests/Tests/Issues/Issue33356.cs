using NUnit.Framework;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.TestCases.Tests.Issues;

public class Issue33356 : _IssuesUITest
{
	public Issue33356(TestDevice device) : base(device) { }

	public override string Issue => "[iOS] Clicking on search suggestions fails to navigate to detail page correctly";

	[Test]
	[Category(UITestCategories.Shell)]
	public void SearchSuggestionTapNavigatesToDetailPage()
	{
		// Wait for the page to load
		App.WaitForElement("WaitForStubControl");

		// Type in the search handler using its placeholder text
		App.EnterText("Type to search (try 'cat')", "cat");

		// Wait for suggestions to appear
		App.WaitForElement("SuggestionLabel", timeout: TimeSpan.FromSeconds(5));
		
		// Find and tap the Bengal Cat suggestion
		var suggestions = App.FindElements("SuggestionLabel");
		var bengalSuggestion = suggestions.FirstOrDefault(s => s.GetText()?.Contains("Bengal", StringComparison.Ordinal) == true);
		
		Assert.That(bengalSuggestion, Is.Not.Null, "Bengal Cat suggestion should be visible");
		bengalSuggestion!.Tap();

		// Verify navigation to detail page occurred
		App.WaitForElement("NavigationSuccess", timeout: TimeSpan.FromSeconds(5));
		
		// Verify the correct animal was passed
		var animalName = App.FindElement("AnimalName").GetText();
		Assert.That(animalName, Is.EqualTo("Bengal Cat"), "Detail page should show Bengal Cat");
	}

	[Test]
	[Category(UITestCategories.Shell)]
	public void BackNavigationFromDetailPageWorks()
	{
		// First navigate to detail page via search suggestion
		App.WaitForElement("WaitForStubControl");
		
		// Type in the search handler
		App.EnterText("Type to search (try 'cat')", "cat");
		
		App.WaitForElement("SuggestionLabel", timeout: TimeSpan.FromSeconds(5));
		var suggestions = App.FindElements("SuggestionLabel");
		var bengalSuggestion = suggestions.FirstOrDefault(s => s.GetText()?.Contains("Bengal", StringComparison.Ordinal) == true);
		Assert.That(bengalSuggestion, Is.Not.Null, "Bengal Cat suggestion should be visible");
		bengalSuggestion!.Tap();

		// Verify we're on detail page
		App.WaitForElement("NavigationSuccess", timeout: TimeSpan.FromSeconds(5));

		// Navigate back
		App.TapBackArrow();

		// Verify we're back on the main page (not an empty page)
		App.WaitForElement("WaitForStubControl", timeout: TimeSpan.FromSeconds(5));
		
		// Verify the status label is visible (confirms page content loaded)
		var statusLabel = App.FindElement("StatusLabel");
		Assert.That(statusLabel, Is.Not.Null, "Should be back on main page with content visible");
	}
}
