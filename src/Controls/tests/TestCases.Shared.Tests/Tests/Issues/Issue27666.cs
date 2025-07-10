using NUnit.Framework;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.TestCases.Tests.Issues;

public class Issue27666 : _IssuesUITest
{
	public override string Issue => "Vertical list and Vertical grid pages have different abnormal behaviors when clicking Update after changing the spacing value";
	const string UpdateItemSpacingButton = "UpdateItemSpacingButton";
	const string ItemsLayoutCollectionView = "ItemsLayoutCollectionView";
	const string NavigationButton = "NavigationButton";
	public Issue27666(TestDevice device) : base(device) { }

	[Test]
	[Category(UITestCategories.CollectionView)]
	public void VerifySpacingUpdateInItemsLayout()
	{
		App.WaitForElement(NavigationButton);
		App.Tap(NavigationButton);
		App.WaitForElement(ItemsLayoutCollectionView);
		App.Tap(UpdateItemSpacingButton);
		App.TapBackArrow();
		App.WaitForElement(NavigationButton);
		App.Tap(NavigationButton);
		App.WaitForElement(ItemsLayoutCollectionView);
		App.Tap(UpdateItemSpacingButton);
		App.WaitForElement(ItemsLayoutCollectionView);
	}
}