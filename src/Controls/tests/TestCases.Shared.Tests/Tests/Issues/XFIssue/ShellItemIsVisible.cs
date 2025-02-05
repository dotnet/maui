using NUnit.Framework;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.TestCases.Tests.Issues;

[Category(UITestCategories.Shell)]
public class ShellItemIsVisible : _IssuesUITest
{
	public ShellItemIsVisible(TestDevice testDevice) : base(testDevice)
	{
	}
	const string TitlePage = "Item Title Page";
	const string TopTab1 = "Top Tab 1";
	const string TopTab2 = "Top Tab 2";

	public override string Issue => "Shell Items IsVisible Test";

	[Test, Order(1)]
	public void FlyoutItemVisible()
	{
		App.WaitForElement("ToggleFlyoutItem3");
		App.WaitForElement("ToggleFlyoutItem3");
		App.Tap("ToggleFlyoutItem3");
		App.ShowFlyout();
		App.WaitForElement("Item2 Flyout");
		App.WaitForNoElement("Item3 Flyout");
	}

	[Test, Order(6)]
	public void HideActiveShellContent()
	{
		App.WaitForElement("Item1 Flyout");
		App.Tap("Item1 Flyout");
		App.WaitForElementTillPageNavigationSettled(TitlePage);
		App.TapTab(TitlePage, true);
		App.WaitForElementTillPageNavigationSettled("ToggleItem1");
		App.Tap("ToggleItem1");
		App.WaitForElement("Welcome to Tab 1");
		App.WaitForNoElement("ToggleItem1");
	}


	[Test, Order(5)]
	public void HideFlyoutItem()
	{
		App.TapInShellFlyout("Item2 Flyout");
		App.WaitForElement("ToggleItem1");
		App.ShowFlyout();
		App.WaitForElement("Item2 Flyout");
		App.Tap("Item2 Flyout");
		App.WaitForElementTillPageNavigationSettled("AllVisible");
		App.Tap("AllVisible");
		App.WaitForElementTillPageNavigationSettled("ToggleItem2");
		App.Tap("ToggleItem2");
		App.TapTab(TitlePage);
		App.ShowFlyout();
		App.WaitForElement("Item1 Flyout");
		App.WaitForNoElement("Item2 Flyout");
	}

#if !IOS // This test fails on CI for iOS. But in local it not replicate.
	[Test, Order(2)]
	public void ClearAndRecreateShellElements()
	{
		App.WaitForElement("Item1 Flyout");
		App.Tap("Item1 Flyout");
		App.WaitForElement("ClearAndRecreate");
		App.Tap("ClearAndRecreate");
		App.WaitForElementTillPageNavigationSettled("ClearAndRecreate");
		App.Tap("ClearAndRecreate");
	}
#endif

#if !WINDOWS // The test fails on Windows and throws an exception: "Navigation still processing" when tapping ClearAndRecreate after tapping ToggleItem1. MoreInformation:https://github.com/dotnet/maui/issues/17608
	[Test, Order(4)]
	public void ClearAndRecreateFromSecondaryPage()
	{
		App.WaitForElement("ClearAndRecreate");
		App.ShowFlyout();
		App.WaitForElement("Item2 Flyout");
		App.Tap("Item2 Flyout");
		App.WaitForElementTillPageNavigationSettled("ToggleItem1");
		App.Tap("ToggleItem1");
		App.WaitForElementTillPageNavigationSettled("ClearAndRecreate");
		App.Tap("ClearAndRecreate");
		App.TapTab(TopTab2);
		App.TapTab(TopTab1);
	}
#endif
	[Test, Order(3)]
	public void ClearAndRecreateShellContent()
	{
#if IOS // TODO: Remove this once enabled the test order 2 in iOS
		App.WaitForElement("Item1 Flyout");
		App.Tap("Item1 Flyout");
#endif
		App.WaitForElementTillPageNavigationSettled("ClearAndRecreateShellContent");
		App.Tap("ClearAndRecreateShellContent");
		App.WaitForElement("ClearAndRecreate");
		App.Tap("ClearAndRecreate");
	}
}