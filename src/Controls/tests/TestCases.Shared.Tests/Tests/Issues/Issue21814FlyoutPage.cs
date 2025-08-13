using NUnit.Framework;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.TestCases.Tests.Issues;

public class Issue21814FlyoutPage : _IssuesUITest
{
	const string FlyoutContent1 = "FlyoutContent1";
	const string FlyoutContent2 = "FlyoutContent2";
	const string FlyoutContent3 = "FlyoutContent3";

	const string FlyoutItem1OnNavigatedToLabel = "FlyoutItem1OnNavigatedToLabel";
	const string FlyoutItem1OnNavigatingFromLabel = "FlyoutItem1OnNavigatingFromLabel";
	const string FlyoutItem1OnNavigatedFromLabel = "FlyoutItem1OnNavigatedFromLabel";

	const string FlyoutItem2OnNavigatedToLabel = "FlyoutItem2OnNavigatedToLabel";
	const string FlyoutItem2OnNavigatedFromLabel = "FlyoutItem2OnNavigatedFromLabel";

	const string FlyoutItem3OnNavigatedToLabel = "FlyoutItem3OnNavigatedToLabel";
	const string FlyoutItem3OnNavigatedFromLabel = "FlyoutItem3OnNavigatedFromLabel";

	const string Item1MenuItem = "Item 1";
	const string Item2MenuItem = "Item 2";
	const string Item3MenuItem = "Item 3";

	public Issue21814FlyoutPage(TestDevice device) : base(device)
	{
	}

	public override string Issue => "Add better parameters for navigation args (FlyoutPage)";

	[Test, Order(1)]
	[Category(UITestCategories.FlyoutPage)]
	[Category(UITestCategories.Navigation)]
	public void InitialPageLoadShowsCorrectNavigationInfo()
	{
		// Verify Item 1 is initially loaded and shows correct navigation info
		App.WaitForElement("FlyoutContent1");

		var onNavigatedToText = App.FindElement(FlyoutItem1OnNavigatedToLabel).GetText();
		Assert.That(onNavigatedToText, Does.Contain("PreviousPage: Null"));
		Assert.That(onNavigatedToText, Does.Contain("NavigationType: Replace"));

		// Initially, OnNavigatingFrom and OnNavigatedFrom should show "-"
		var onNavigatingFromText = App.FindElement(FlyoutItem1OnNavigatingFromLabel).GetText();
		Assert.That(onNavigatingFromText, Does.Contain("-"));

		var onNavigatedFromText = App.FindElement(FlyoutItem1OnNavigatedFromLabel).GetText();
		Assert.That(onNavigatedFromText, Is.EqualTo("-"));
	}

	[Test, Order(2)]
	[Category(UITestCategories.FlyoutPage)]
	[Category(UITestCategories.Navigation)]
	public void NavigationFromItem1ToItem2ShowsCorrectParameters()
	{
		// Start on Item 1
		App.WaitForElement(FlyoutContent1);

		// Open flyout menu and navigate to Item 2
		OpenFlyoutMenu();
		App.Tap(Item2MenuItem);
		App.WaitForElement(FlyoutContent2);

		// Verify Item 2 navigation parameters
		var item2OnNavigatedToText = App.FindElement(FlyoutItem2OnNavigatedToLabel).GetText();
		Assert.That(item2OnNavigatedToText, Does.Contain("PreviousPage: Issue21814FlyoutItem1"));
		Assert.That(item2OnNavigatedToText, Does.Contain("NavigationType: Replace"));
	}

	[Test, Order(3)]
	[Category(UITestCategories.FlyoutPage)]
	[Category(UITestCategories.Navigation)]
	public void NavigationFromItem2ToItem3ShowsCorrectParameters()
	{
		// Starting on Item 1
#if ANDROID || WINDOWS
		App.TapBackArrow();
#elif IOS || MACCATALYST
		App.TapBackArrow("Item 1");
#endif

		// Navigate to Item 3
		OpenFlyoutMenu();
		App.Tap(Item3MenuItem);
		App.WaitForElement(FlyoutContent3);

		// Verify Item 3 navigation parameters
		var item3OnNavigatedToText = App.FindElement(FlyoutItem3OnNavigatedToLabel).GetText();
		Assert.That(item3OnNavigatedToText, Does.Contain("PreviousPage: Issue21814FlyoutItem1"));
		Assert.That(item3OnNavigatedToText, Does.Contain("NavigationType: Replace"));
	}

	[Test, Order(4)]
	[Category(UITestCategories.FlyoutPage)]
	[Category(UITestCategories.Navigation)]
	public void PopNavigationPageAfterPush()
	{
#if ANDROID || WINDOWS
		App.TapBackArrow();
#elif IOS || MACCATALYST
		App.TapBackArrow("Item 1");
#endif

		// Navigating to Item 2
		OpenFlyoutMenu();
		App.Tap(Item2MenuItem);
		App.WaitForElement(FlyoutContent2);

		// Popping back to Item 1
#if ANDROID || WINDOWS
		App.TapBackArrow();
#elif IOS || MACCATALYST
		App.TapBackArrow("Item 1");
#endif

		// Verifying navigation events for pop from Item 2 to Item 1
		// Popping to Item 1 does not trigger OnNavigatedFrom for Item 1
		// Item 1's OnNavigatedFrom remains from the earlier navigation to Item 2
		var onNavigatedFromText = App.FindElement(FlyoutItem1OnNavigatedFromLabel).GetText();
		Console.WriteLine($"Item2 OnNavigatedFrom: {onNavigatedFromText}");
		Assert.That(onNavigatedFromText, Does.Contain("DestinationPage: Issue21814FlyoutItem2"));
		Assert.That(onNavigatedFromText, Does.Contain("NavigationType: Replace"));
	}

	void OpenFlyoutMenu()
	{
		App.TapFlyoutPageIcon("Flyout Menu");
	}
}