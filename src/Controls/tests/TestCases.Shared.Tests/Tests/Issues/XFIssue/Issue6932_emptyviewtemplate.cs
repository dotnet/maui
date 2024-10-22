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
	//	RunningApp.Screenshot("Screen opens, items are shown");

	//	RunningApp.WaitForElement(_viewModel.LayoutAutomationId);
	//	RunningApp.Tap(_viewModel.ClearAutomationId);
	//	RunningApp.WaitForElement(_viewModel.EmptyTemplateAutomationId);

	//	RunningApp.Screenshot("Empty view is visible");
	//}

	//[Test]
	//[FailsOnMauiIOS]
	//public void EmptyViewTemplateBecomesVisibleWhenItemsSourceIsEmptiedOneByOne()
	//{
	//	RunningApp.Screenshot("Screen opens, items are shown");

	//	RunningApp.WaitForElement(_viewModel.LayoutAutomationId);

	//	for (var i = 0; i < _viewModel.ItemsSource.Count; i++)
	//		RunningApp.Tap(_viewModel.RemoveAutomationId);

	//	RunningApp.WaitForElement(_viewModel.EmptyTemplateAutomationId);

	//	RunningApp.Screenshot("Empty view is visible");
	//}

	//[Test]
	//[FailsOnMauiIOS]
	//public void EmptyViewTemplateHidesWhenItemsSourceIsFilled()
	//{
	//	RunningApp.Screenshot("Screen opens, items are shown");

	//	RunningApp.WaitForElement(_viewModel.LayoutAutomationId);
	//	RunningApp.Tap(_viewModel.ClearAutomationId);
	//	RunningApp.WaitForElement(_viewModel.EmptyTemplateAutomationId);

	//	RunningApp.Screenshot("Items are cleared, empty view visible");

	//	RunningApp.Tap(_viewModel.AddAutomationId);
	//	RunningApp.WaitForNoElement(_viewModel.EmptyTemplateAutomationId);

	//	RunningApp.Screenshot("Item is added, empty view is not visible");
	//}
}