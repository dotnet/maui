#if TEST_FAILS_ON_ANDROID
// https://github.com/dotnet/maui/issues/28676
using NUnit.Framework;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.TestCases.Tests.Issues;

public class Issue28509 : _IssuesUITest
{
	public override string Issue => "Dynamically Setting Header and Footer in CV2 Does Not Update Properly";

	public Issue28509(TestDevice device) : base(device)
	{
	}

	[Test, Order(1)]
	[Category(UITestCategories.CollectionView)]
	public void UpdateGroupHeaderAndFooterDynamically()
	{
		App.WaitForElement("GroupHeaderFooterButton");
		App.Click("GroupHeaderFooterButton");
		App.WaitForElement("CollectionView");
		App.Click("ToggleHeaderButton");
		App.WaitForElement("GroupHeaderTemplate Changed");
		App.Click("ToggleFooterButton");
		App.WaitForElement("GroupFooterTemplate Changed");
		App.TapBackArrow();
	}

	[Test, Order(2)]
	[Category(UITestCategories.CollectionView)]
	public void UpdateHeaderFooterTemplateDynamically()
	{
		App.WaitForElement("ItemsViewTemplatedHeaderFooterButton");
		App.Click("ItemsViewTemplatedHeaderFooterButton");
		App.WaitForElement("CollectionView");
		App.Click("ToggleHeaderButton");
		App.WaitForElement("HeaderTemplate Changed");
		App.Click("ToggleFooterButton");
		App.WaitForElement("FooterTemplate Changed");
		App.TapBackArrow();
	}

	[Test, Order(3)]
	[Category(UITestCategories.CollectionView)]
	public void UpdateHeaderFooterDynamically()
	{
		App.WaitForElement("ItemsViewHeaderFooterButton");
		App.Click("ItemsViewHeaderFooterButton");
		App.WaitForElement("CollectionView");
		App.Click("ToggleHeaderButton");
		App.WaitForElement("Header Changed");
		App.Click("ToggleFooterButton");
		App.WaitForElement("Footer Changed");
	}
}
#endif