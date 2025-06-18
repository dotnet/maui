using NUnit.Framework;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.TestCases.Tests.Issues;

internal class Issue25726 : _IssuesUITest
{
#if ANDROID || WINDOWS
	private const string BackButtonIdentifier = "";
#else
    private const string BackButtonIdentifier = "Page 1";
#endif

	public Issue25726(TestDevice device) : base(device) { }

	public override string Issue => "NullReferenceException in WillMoveToParentViewController When Removing Page During Navigation on iOS";

	[Test]
	[Category(UITestCategories.Navigation)]
	public void NavigationShouldNotCrashWhenRemovingPageDuringNavigation()
	{
		App.WaitForElement("NavigateToPage2Button");
		App.Tap("NavigateToPage2Button");

		App.WaitForElement("NavigateToPage3Button");
		App.Tap("NavigateToPage3Button");

		// Ensure the label on Page 3 is present to verify navigation completed successfully
		// If the app crashes during navigation, the test will fail before reaching this line.
		App.WaitForElement("Page3Label");

		App.TapBackArrow(BackButtonIdentifier);

		// Ensure we navigate back to Page 1 successfully
		App.WaitForElement("NavigateToPage2Button");
	}
}