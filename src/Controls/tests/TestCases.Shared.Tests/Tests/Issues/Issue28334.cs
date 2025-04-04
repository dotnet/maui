using NUnit.Framework;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.TestCases.Tests.Issues;
public class Issue28334 : _IssuesUITest
{
	public Issue28334(TestDevice device) : base(device) { }

	public override string Issue => "[Windows] EmptyViewTemplate not displayed when ItemsSource is set to Null";

	[Test, Order(1)]
	[Category(UITestCategories.CollectionView)]
	public void EmptyViewShouldRenderProperly()
	{
		App.WaitForElement("EmptyViewButton");
		App.Tap("EmptyViewButton");
		App.Tap("ItemSourceButton");
		VerifyScreenshot();
	}

	[Test, Order(2)]
	[Category(UITestCategories.CollectionView)]
	public void EmptyViewTemplateShouldRenderProperly()
	{
		App.WaitForElement("EmptyViewTemplateButton");
		App.Tap("EmptyViewTemplateButton");
		VerifyScreenshot();
	}

	[Test, Order(3)]
	[Category(UITestCategories.CollectionView)]
	public void ReAddItemSourceShouldRenderProperly()
	{
		App.WaitForElement("ReAddItemSourceButton");
		App.Tap("ReAddItemSourceButton");
		VerifyScreenshot();
	}

}