using Xunit;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.TestCases.Tests.Issues;

[Category(UITestCategories.Shell)]
public class Issue6784 : _IssuesUITest
{
	public Issue6784(TestDevice testDevice) : base(testDevice)
	{
	}

	public override string Issue => "ShellItem.CurrentItem is not set when selecting shell section aggregated in more tab";

	[Fact]
	public void CurrentItemIsSetWhenSelectingShellSectionAggregatedInMoreTab()
	{
		App.WaitForElement("More");
		App.Tap("More");
		App.WaitForElement("Tab 11");
		App.Tap("Tab 11");
		App.WaitForElementTillPageNavigationSettled("Success");
	}

	[Fact]
	public void OneMoreControllerOpensOnFirstClick()
	{
		App.WaitForElement("More");
		App.Tap("More");
		App.WaitForElement("Tab 11");
		App.Tap("Tab 11");
		App.WaitForElement("Tab 4");
		App.Tap("Tab 4");
		App.WaitForElement("More");
		App.Tap("More");
		// On iOS and Mac Catalyst, the first 'more' tab goes back to tab 11, the second click goes to the 'more' menu
#if IOS || MACCATALYST
		App.Tap("More");
#endif
		App.WaitForElement("Tab 12");
		App.Tap("Tab 12");
	}

	[Fact]
	public void TwoMoreControllerDoesNotShowEditButton()
	{
		App.WaitForElement("More");
		App.Tap("More");
		App.WaitForElement("Tab 11");

		// The "Edit" element count differs between MacCatalyst and other platforms
		// In MacCatalyst it includes the "Edit" menu item in the macOS menu bar.
		// Note: This different behavior is due to Appium's ability to detect macOS menu items in MacCatalyst apps.
#if MACCATALYST
		Assert.Equal(1, App.FindElements("Edit").Count());
#else
		Assert.Equal(0, App.FindElements("Edit").Count());
#endif
	}
}
