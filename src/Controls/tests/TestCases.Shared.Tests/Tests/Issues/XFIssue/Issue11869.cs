using NUnit.Framework;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.TestCases.Tests.Issues;

public class Issue11869 : _IssuesUITest
{

	// In Android the top tab title was displayed with uppercase so we need to use uppercase string to find the element
#if ANDROID
	const string TopTab2 = "TOPTAB2";
	const string TopTab3 = "TOPTAB3";

#else
    const string TopTab2 = "TopTab2";
	const string TopTab3 = "TopTab3";
#endif
	const string HideTop2 = "HideTop2";
	const string HideTop3 = "HideTop3";
	const string HideBottom2 = "HideBottom2";
	const string HideBottom3 = "HideBottom3";
	const string Tab2 = "Tab 2";
	const string Tab3 = "Tab 3";
	const string ShowAllTabs = "ShowAllTabs";



	public Issue11869(TestDevice testDevice) : base(testDevice)
	{
	}

	public override string Issue => "[Bug] ShellContent.IsVisible issue on Android";

	[Test]
	[Category(UITestCategories.Shell)]
	public void IsVisibleWorksForShowingHidingTabs()
	{
		// Ignored on Windows: The BottomTabs are displayed as a popup with a dropdown icon on Windows. 
		// This causes a visibility issue where the content does not function as expected, a bug has been logged to address this behavior. 
		// Once the issue is fixed, the test case should be re-enabled for Windows. https://github.com/dotnet/maui/issues/25913
#if !WINDOWS
		App.WaitForElement(TopTab2);
		App.Tap(HideTop2);
		App.WaitForNoElement(TopTab2);

		App.WaitForElement(TopTab3);
		App.Tap(HideTop3);
		App.WaitForNoElement(TopTab3);
#endif

		App.WaitForElement(Tab2);
		App.Tap(HideBottom2);
		App.WaitForNoElement(Tab2);

		App.WaitForElement(Tab3);
		App.Tap(HideBottom3);
		App.WaitForNoElement(Tab3);

		App.Tap(ShowAllTabs);
#if !WINDOWS
		App.WaitForElement(TopTab2);
		App.WaitForElement(TopTab3);
#endif
		App.WaitForElement(Tab2);
		App.WaitForElement(Tab3);
	}
}