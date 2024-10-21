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
	//	RunningApp.Tap("ToggleFlyoutItem3");
	//	ShowFlyout();
	//	RunningApp.WaitForElement("Item2 Flyout");
	//	RunningApp.WaitForNoElement("Item3 Flyout");
	//}

	//[Test]
	//public void HideActiveShellContent()
	//{
	//	RunningApp.Tap("ToggleItem1");
	//	RunningApp.WaitForElement("Welcome to Tab 1");
	//	RunningApp.WaitForNoElement("ToggleItem1");
	//}

	//[Test]
	//public void HideFlyoutItem()
	//{
	//	RunningApp.WaitForElement("ToggleItem1");
	//	ShowFlyout();
	//	RunningApp.WaitForElement("Item2 Flyout");
	//	RunningApp.Tap("Item2 Flyout");
	//	RunningApp.Tap("AllVisible");
	//	RunningApp.Tap("ToggleItem2");
	//	ShowFlyout();
	//	RunningApp.WaitForElement("Item1 Flyout");
	//	RunningApp.WaitForNoElement("Item2 Flyout");
	//}

	//[Test]
	//public void ClearAndRecreateShellElements()
	//{
	//	RunningApp.WaitForElement("ClearAndRecreate");
	//	RunningApp.Tap("ClearAndRecreate");
	//	RunningApp.WaitForElement("ClearAndRecreate");
	//	RunningApp.Tap("ClearAndRecreate");
	//}


	//[Test]
	//public void ClearAndRecreateFromSecondaryPage()
	//{
	//	RunningApp.WaitForElement("ClearAndRecreate");
	//	ShowFlyout();
	//	RunningApp.Tap("Item2 Flyout");
	//	RunningApp.Tap("ToggleItem1");
	//	RunningApp.Tap("ClearAndRecreate");
	//	RunningApp.Tap("Top Tab 2");
	//	RunningApp.Tap("Top Tab 1");
	//}

	//[Test]
	//public void ClearAndRecreateShellContent()
	//{
	//	RunningApp.WaitForElement("ClearAndRecreateShellContent");
	//	RunningApp.Tap("ClearAndRecreateShellContent");
	//	RunningApp.WaitForElement("ClearAndRecreate");
	//	RunningApp.Tap("ClearAndRecreate");
	//}
}