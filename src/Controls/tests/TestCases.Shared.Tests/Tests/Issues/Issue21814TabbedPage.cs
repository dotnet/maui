using NUnit.Framework;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.TestCases.Tests.Issues;

public class Issue21814TabbedPage : _IssuesUITest
{
	const string Tab1Content = "Tab1Content";
	const string Tab2Content = "Tab2Content";
	const string Tab3Content = "Tab3Content";

	const string Tab1OnNavigatedToLabel = "Tab1OnNavigatedToLabel";
	const string Tab1OnNavigatingFromLabel = "Tab1OnNavigatingFromLabel";
	const string Tab1OnNavigatedFromLabel = "Tab1OnNavigatedFromLabel";

	const string Tab2OnNavigatedToLabel = "Tab2OnNavigatedToLabel";
	const string Tab2OnNavigatedFromLabel = "Tab2OnNavigatedFromLabel";

	const string Tab3OnNavigatedToLabel = "Tab3OnNavigatedToLabel";
	const string Tab3OnNavigatedFromLabel = "Tab3OnNavigatedFromLabel";

	public Issue21814TabbedPage(TestDevice device) : base(device)
	{
	}

	public override string Issue => "Add better parameters for navigation args (TabbedPage)";

	[Test, Order(1)]
	[Category(UITestCategories.Navigation)]
	[Category(UITestCategories.TabbedPage)]
	public void InitialTabLoadShowsCorrectNavigationInfo()
	{
		// Verify Tab 1 is initially loaded and shows correct navigation info
		App.WaitForElement(Tab1Content);
		
		var onNavigatedToText = App.FindElement(Tab1OnNavigatedToLabel).GetText();
		Assert.That(onNavigatedToText, Does.Contain("PreviousPage: Null"));
		Assert.That(onNavigatedToText, Does.Contain("NavigationType: Replace"));

		// Initially, OnNavigatingFrom and OnNavigatedFrom should show "-"
		var onNavigatingFromText = App.FindElement(Tab1OnNavigatingFromLabel).GetText();
		Assert.That(onNavigatingFromText, Does.Contain("-"));
		
		var onNavigatedFromText = App.FindElement(Tab1OnNavigatedFromLabel).GetText();
		Assert.That(onNavigatedFromText, Is.EqualTo("-"));
	}

	[Test, Order(2)]
	[Category(UITestCategories.TabbedPage)]
	[Category(UITestCategories.Navigation)]
	public void NavigationFromTab1ToTab2ShowsCorrectParameters()
	{
		// Start on Tab 1
		App.WaitForElement(Tab1Content);

		// Navigate to Tab 2
		App.TapTab("Tab 2");
		App.WaitForElement(Tab2Content);

		// Verify Tab 2 navigation parameters
		var tab2OnNavigatedToText = App.FindElement(Tab2OnNavigatedToLabel).GetText();
		Assert.That(tab2OnNavigatedToText, Does.Contain("PreviousPage: Issue21814TabItem1"));
		Assert.That(tab2OnNavigatedToText, Does.Contain("NavigationType: Replace"));

		// Go back to Tab 1 to check its OnNavigatingFrom and OnNavigatedFrom
		App.TapTab("Tab 1");
		App.WaitForElement(Tab1Content);
		
		var tab1OnNavigatedFromText = App.FindElement(Tab1OnNavigatedFromLabel).GetText();
		Assert.That(tab1OnNavigatedFromText, Does.Contain("DestinationPage: Issue21814TabItem2"));
	}

	[Test, Order(3)]
	[Category(UITestCategories.TabbedPage)]
	[Category(UITestCategories.Navigation)]
	public void NavigationFromTab2ToTab3ShowsCorrectParameters()
	{
		// Navigate to Tab 2 first
		App.TapTab("Tab 2");
		App.WaitForElement(Tab2Content);

		// Then navigate to Tab 3
		App.TapTab("Tab 3");
		App.WaitForElement(Tab3Content);

		// Verify Tab 3 navigation parameters
		var tab3OnNavigatedToText = App.FindElement(Tab3OnNavigatedToLabel).GetText();
		Assert.That(tab3OnNavigatedToText, Does.Contain("PreviousPage: Issue21814TabItem2"));
		Assert.That(tab3OnNavigatedToText, Does.Contain("NavigationType: Replace"));

		// Go back to Tab 2 to verify its navigation from parameters
		App.TapTab("Tab 2");
		App.WaitForElement(Tab2Content);
		
		var tab2OnNavigatedFromText = App.FindElement(Tab2OnNavigatedFromLabel).GetText();
		Assert.That(tab2OnNavigatedFromText, Does.Contain("DestinationPage: Issue21814TabItem3"));
	}

	[Test, Order(4)]
	[Category(UITestCategories.TabbedPage)]
	[Category(UITestCategories.Navigation)]
	public void NavigationFromTab1ToTab3SkippingTab2ShowsCorrectParameters()
	{
		// Start on Tab 1
		App.TapTab("Tab 1");
		App.WaitForElement(Tab1Content);

		// Navigate directly to Tab 3 (skipping Tab 2)
		App.TapTab("Tab 3");
		App.WaitForElement(Tab3Content);

		// Verify Tab 3 shows Tab 1 as previous page
		var tab3OnNavigatedToText = App.FindElement(Tab3OnNavigatedToLabel).GetText();
		Assert.That(tab3OnNavigatedToText, Does.Contain("PreviousPage: Issue21814TabItem1"));
		Assert.That(tab3OnNavigatedToText, Does.Contain("NavigationType: Replace"));

		// Verify Tab 1's navigation from parameters point to Tab 3
		App.TapTab("Tab 1");
		App.WaitForElement(Tab1Content);
		
		var tab1OnNavigatedFromText = App.FindElement(Tab1OnNavigatedFromLabel).GetText();
		Assert.That(tab1OnNavigatedFromText, Does.Contain("DestinationPage: Issue21814TabItem3"));
	}
	
	[Test, Order(5)]
	[Category(UITestCategories.TabbedPage)]
	[Category(UITestCategories.Navigation)]
	[Description("Edge case: Verify behavior when tapping the same tab multiple times")]
	public void TappingSameTabMultipleTimesDoesNotTriggerNavigationEvents()
	{
		// Start on Tab 1
		App.WaitForElement(Tab1Content);
            
		// Get initial state
		var initialOnNavigatedFrom = App.FindElement(Tab1OnNavigatedFromLabel).GetText();
            
		// Tap Tab 1 multiple times (should not trigger navigation events)
		App.TapTab("Tab 1");
		App.TapTab("Tab 1");
		App.TapTab("Tab 1");
            
		// Verify no navigation events were triggered
		var currentOnNavigatedFrom = App.FindElement(Tab1OnNavigatedFromLabel).GetText();
		Assert.That(currentOnNavigatedFrom, Is.EqualTo(initialOnNavigatedFrom));
	}
}