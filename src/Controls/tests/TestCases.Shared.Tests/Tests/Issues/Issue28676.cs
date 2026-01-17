using NUnit.Framework;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.TestCases.Tests.Issues;

[Category(UITestCategories.CollectionView)]
public class Issue28676 : _IssuesUITest
{
	public override string Issue => "Android Dynamic Updates to CollectionView Header and Footer Are Not Displayed";

	public Issue28676(TestDevice device) : base(device)
	{
	}

	[Test, Order(1)]
	public void CollectionViewHeaderShouldChangeDynamically()
	{
		App.WaitForElement("Issue28676Header");
		App.WaitForElement("Issue28676UpdateHeaderButton");
		App.Tap("Issue28676UpdateHeaderButton");
		App.WaitForElement("UpdatedCollectionViewHeader");
	}
	[Test, Order(2)]
	public void CollectionViewFooterShouldChangeDynamically()
	{
		App.WaitForElement("Issue28676Footer");
		App.WaitForElement("Issue28676UpdateFooterButton");
		App.Tap("Issue28676UpdateFooterButton");
		App.WaitForElement("UpdatedCollectionViewFooter");
	}

	[Test, Order(3)]
	public void CollectionViewHeaderViewShouldChangeDynamically()
	{
		App.WaitForElement("UpdatedCollectionViewHeader");
		App.WaitForElement("Issue28676ChangeHeaderView");
		App.Tap("Issue28676ChangeHeaderView");
		App.WaitForElement("Updated HeaderView");
	}

	[Test, Order(4)]
	public void CollectionViewFooterViewShouldChangeDynamically()
	{
		App.WaitForElement("UpdatedCollectionViewFooter");
		App.WaitForElement("Issue28676ChangeFooterView");
		App.Tap("Issue28676ChangeFooterView");
		App.WaitForElement("Updated FooterView");
	}

}