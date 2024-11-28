using NUnit.Framework;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.TestCases.Tests.Issues;

public class Issue7181 : _IssuesUITest
{
	public Issue7181(TestDevice testDevice) : base(testDevice)
	{
	}

	public override string Issue => "[Bug] Cannot update ToolbarItem text and icon";

	//[Test]
	//[Category(UITestCategories.Shell)]
	//public void ShellToolbarItemTests()
	//{
	//	var count = 0;
	//	var toolbarButton = App.WaitForElement(ToolbarBtn);
	//	Assert.AreEqual(DefaultToolbarItemText, toolbarButton[0].ReadText());

	//	for (int i = 0; i < 5; i++)
	//	{
	//		App.Tap(ToolbarBtn);

	//		toolbarButton = App.WaitForElement(ToolbarBtn);
	//		Assert.AreEqual($"{AfterClickToolbarItemText} {count++}", toolbarButton[0].ReadText());
	//	}

	//	App.Tap(SetToolbarIconBtn);
	//}
}