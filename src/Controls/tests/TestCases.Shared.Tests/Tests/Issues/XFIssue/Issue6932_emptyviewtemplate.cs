using NUnit.Framework;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.TestCases.Tests.Issues;

[Category(UITestCategories.Layout)]
public class Issue6932_emptyviewtemplate : _IssuesUITest
{
	public Issue6932_emptyviewtemplate(TestDevice testDevice) : base(testDevice)
	{
	}

	public override string Issue => "EmptyView for BindableLayout (template)";

	//[Test]
	//[FailsOnMauiIOS]
	//public void EmptyViewTemplateBecomesVisibleWhenItemsSourceIsCleared()
	//{
	//	App.Screenshot("Screen opens, items are shown");

	//	App.WaitForElement(_viewModel.LayoutAutomationId);
	//	App.Tap(_viewModel.ClearAutomationId);
	//	App.WaitForElement(_viewModel.EmptyTemplateAutomationId);

	//	App.Screenshot("Empty view is visible");
	//}

	//[Test]
	//[FailsOnMauiIOS]
	//public void EmptyViewTemplateBecomesVisibleWhenItemsSourceIsEmptiedOneByOne()
	//{
	//	App.Screenshot("Screen opens, items are shown");

	//	App.WaitForElement(_viewModel.LayoutAutomationId);

	//	for (var i = 0; i < _viewModel.ItemsSource.Count; i++)
	//		App.Tap(_viewModel.RemoveAutomationId);

	//	App.WaitForElement(_viewModel.EmptyTemplateAutomationId);

	//	App.Screenshot("Empty view is visible");
	//}

	//[Test]
	//[FailsOnMauiIOS]
	//public void EmptyViewTemplateHidesWhenItemsSourceIsFilled()
	//{
	//	App.Screenshot("Screen opens, items are shown");

	//	App.WaitForElement(_viewModel.LayoutAutomationId);
	//	App.Tap(_viewModel.ClearAutomationId);
	//	App.WaitForElement(_viewModel.EmptyTemplateAutomationId);

	//	App.Screenshot("Items are cleared, empty view visible");

	//	App.Tap(_viewModel.AddAutomationId);
	//	App.WaitForNoElement(_viewModel.EmptyTemplateAutomationId);

	//	App.Screenshot("Item is added, empty view is not visible");
	//}
}