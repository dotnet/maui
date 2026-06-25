using NUnit.Framework;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.TestCases.Tests.Issues;

public class Issue27959 : _IssuesUITest
{
	public override string Issue => "Dynamically toggling the Header/Footer between a null and a non-null value in CollectionView is not working";

	public Issue27959(TestDevice device) : base(device)
	{
	}

	[Test, Order(1)]
	[Category(UITestCategories.CollectionView)]
	public void EmptyViewHeaderFooterToggleNullToNonNull()
	{
		App.WaitForElement("EmptyViewButton").Click();
		App.WaitForElement("ToggleHeaderButton");
		App.Click("ToggleHeaderButton");
		App.WaitForNoElement("Header");
		App.Click("ToggleHeaderButton");
		App.WaitForElement("Header");
		App.Click("ToggleFooterButton");
		App.WaitForNoElement("Footer");
		App.Click("ToggleFooterButton");
		App.WaitForElement("Footer");
		App.TapBackArrow();
	}

	[Test, Order(2)]
	[Category(UITestCategories.CollectionView)]
	public void EmptyViewHeaderFooterTemplateToggleNullToNonNull()
	{
		App.WaitForElement("EmptyViewViewTemplatedButton").Click();
		App.WaitForElement("ToggleHeaderTemplateButton");
		App.Click("ToggleHeaderTemplateButton");
		App.WaitForNoElement("HeaderTemplate");
		App.Click("ToggleHeaderTemplateButton");
		App.WaitForElement("HeaderTemplate");
		App.TapBackArrow();
	}

	[Test, Order(3)]
	[Category(UITestCategories.CollectionView)]
	public void ItemsViewHeaderFooterToggleNullToNonNull()
	{
		App.WaitForElement("ItemsViewButton").Click();
		App.WaitForElement("ItemsViewToggleHeaderButton");
		App.Click("ItemsViewToggleHeaderButton");
		App.WaitForNoElement("ItemsViewHeader");
		App.Click("ItemsViewToggleHeaderButton");
		App.WaitForElement("ItemsViewHeader");
		App.Click("ItemsViewToggleFooterButton");
		App.WaitForNoElement("ItemsViewFooter");
		App.Click("ItemsViewToggleFooterButton");
		App.WaitForElement("ItemsViewFooter");
		App.TapBackArrow();
	}

	[Test, Order(4)]
	[Category(UITestCategories.CollectionView)]
	public void ItemsViewHeaderFooterTemplatedToggleNullToNonNull()
	{
		App.WaitForElement("ItemsViewTemplatedButton").Click();
		App.WaitForElement("ToggleHeaderTemplateButton");
		App.Click("ToggleHeaderTemplateButton");
		App.WaitForNoElement("ItemsHeaderTemplate");
		App.Click("ToggleHeaderTemplateButton");
		App.WaitForElement("ItemsHeaderTemplate");
		App.Click("ToggleFooterTemplateButton");
		App.WaitForNoElement("ItemsFooterTemplate");
		App.Click("ToggleFooterTemplateButton");
		App.WaitForElement("ItemsFooterTemplate");
	}
}
