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

	public override string Issue => "Shell Items IsVisible Test";

	//	[Test]
	//public void FlyoutItemVisible()
	//{
	//	App.Tap("ToggleFlyoutItem3");
	//	ShowFlyout();
	//	App.WaitForElement("Item2 Flyout");
	//	App.WaitForNoElement("Item3 Flyout");
	//}

	//[Test]
	//public void HideActiveShellContent()
	//{
	//	App.Tap("ToggleItem1");
	//	App.WaitForElement("Welcome to Tab 1");
	//	App.WaitForNoElement("ToggleItem1");
	//}

	//[Test]
	//public void HideFlyoutItem()
	//{
	//	App.WaitForElement("ToggleItem1");
	//	ShowFlyout();
	//	App.WaitForElement("Item2 Flyout");
	//	App.Tap("Item2 Flyout");
	//	App.Tap("AllVisible");
	//	App.Tap("ToggleItem2");
	//	ShowFlyout();
	//	App.WaitForElement("Item1 Flyout");
	//	App.WaitForNoElement("Item2 Flyout");
	//}

	//[Test]
	//public void ClearAndRecreateShellElements()
	//{
	//	App.WaitForElement("ClearAndRecreate");
	//	App.Tap("ClearAndRecreate");
	//	App.WaitForElement("ClearAndRecreate");
	//	App.Tap("ClearAndRecreate");
	//}


	//[Test]
	//public void ClearAndRecreateFromSecondaryPage()
	//{
	//	App.WaitForElement("ClearAndRecreate");
	//	ShowFlyout();
	//	App.Tap("Item2 Flyout");
	//	App.Tap("ToggleItem1");
	//	App.Tap("ClearAndRecreate");
	//	App.Tap("Top Tab 2");
	//	App.Tap("Top Tab 1");
	//}

	//[Test]
	//public void ClearAndRecreateShellContent()
	//{
	//	App.WaitForElement("ClearAndRecreateShellContent");
	//	App.Tap("ClearAndRecreateShellContent");
	//	App.WaitForElement("ClearAndRecreate");
	//	App.Tap("ClearAndRecreate");
	//}
}