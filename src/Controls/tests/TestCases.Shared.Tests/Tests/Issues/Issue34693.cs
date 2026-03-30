#if IOS || MACCATALYST // The fix is available on iOS and macCatalyst only. Android is fixed in https://github.com/dotnet/maui/pull/29545. A fix for Windows is available in an open PR (https://github.com/dotnet/maui/pull/29441), so the test is restricted on Windows and Android for now.
using NUnit.Framework;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.TestCases.Tests.Issues;

public class Issue34693 : _IssuesUITest
{
	public Issue34693(TestDevice testDevice) : base(testDevice) { }

	public override string Issue => "[Shell][iOS & Mac] SearchHandler retains previous page state when switching top tabs";

	[Test]
	[Category(UITestCategories.Shell)]
	public void ShouldUpdateSearchViewOnPageNavigation()
	{
		App.WaitForElement("MainPageButton");
		App.TapTab("DogsPage");

		// Verify the SearchHandler updated to show the DogsPage placeholder
		var searchHandler = App.GetShellSearchHandler();
		var placeholderText = searchHandler.GetText();
		Assert.That(placeholderText, Does.Contain("dog").IgnoreCase,
			"SearchHandler should show DogsPage placeholder after tab switch");
	}
}
#endif
