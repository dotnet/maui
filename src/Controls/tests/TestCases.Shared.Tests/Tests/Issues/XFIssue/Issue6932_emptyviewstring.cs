using NUnit.Framework;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.TestCases.Tests.Issues;

[Category(UITestCategories.Layout)]
public class Issue6932_emptyviewstring : _IssuesUITest
{
	public Issue6932_emptyviewstring(TestDevice testDevice) : base(testDevice)
	{
	}

	public override string Issue => "EmptyView for BindableLayout (string)";

	//[Test]
	//[FailsOnMauiIOS]
	//public void EmptyViewStringBecomesVisibleWhenItemsSourceIsCleared()
	//{
	//	RunningApp.Screenshot("Screen opens, items are shown");

	//	RunningApp.WaitForElement(_viewModel.LayoutAutomationId);
	//	RunningApp.Tap(_viewModel.ClearAutomationId);
	//	RunningApp.WaitForElement(_viewModel.EmptyViewStringDescription);

	//	RunningApp.Screenshot("Empty view is visible");
	//}

	//[Test]
	//[FailsOnMauiIOS]
	//public void EmptyViewStringBecomesVisibleWhenItemsSourceIsEmptiedOneByOne()
	//{
	//	RunningApp.Screenshot("Screen opens, items are shown");

	//	RunningApp.WaitForElement(_viewModel.LayoutAutomationId);

	//	for (var i = 0; i < _viewModel.ItemsSource.Count; i++)
	//		RunningApp.Tap(_viewModel.RemoveAutomationId);

	//	RunningApp.WaitForElement(_viewModel.EmptyViewStringDescription);

	//	RunningApp.Screenshot("Empty view is visible");
	//}

	//[Test]
	//[FailsOnMauiIOS]
	//public void EmptyViewStringHidesWhenItemsSourceIsFilled()
	//{
	//	RunningApp.Screenshot("Screen opens, items are shown");

	//	RunningApp.WaitForElement(_viewModel.LayoutAutomationId);
	//	RunningApp.Tap(_viewModel.ClearAutomationId);
	//	RunningApp.WaitForElement(_viewModel.EmptyViewStringDescription);

	//	RunningApp.Screenshot("Items are cleared, empty view visible");

	//	RunningApp.Tap(_viewModel.AddAutomationId);
	//	RunningApp.WaitForNoElement(_viewModel.EmptyViewStringDescription);

	//	RunningApp.Screenshot("Item is added, empty view is not visible");
	//}
}