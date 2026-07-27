using NUnit.Framework;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.TestCases.Tests.Issues;

public class Issue35005 : _IssuesUITest
{
	public Issue35005(TestDevice device) : base(device) { }

	public override string Issue => "SearchHandler Query not shown on iOS/MacCatalyst at load time";

	[Test]
	[Category(UITestCategories.Shell)]
	public void SearchHandlerQueryShouldBeVisibleOnLoad()
	{
		// Wait for the page to load
		App.WaitForElement("Issue35005StatusLabel");

		// Get the search bar element
		var searchHandler = App.GetShellSearchHandler();

		// Verify that the search bar shows the pre-set Query value, not just the placeholder
		var text = searchHandler.GetText();
		Assert.That(text, Is.EqualTo("InitialQuery"),
			"SearchHandler Query set at load time should be visible in the search bar on all platforms.");
	}
}
