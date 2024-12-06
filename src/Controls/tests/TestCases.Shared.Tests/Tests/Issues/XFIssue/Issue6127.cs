using NUnit.Framework;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.TestCases.Tests.Issues;

public class Issue6127 : _IssuesUITest
{
	public Issue6127(TestDevice testDevice) : base(testDevice)
	{
	}

	public override string Issue => "[Bug] ToolbarItem Order property ignored";

	//[Test]
	//[Category(UITestCategories.ToolbarItem)]
	//public void Issue6127Test() 
	//{
	//	App.WaitForElement(PrimaryToolbarIcon);

	//	App.Tap("More options");
	//	App.WaitForElement(SecondaryToolbarIcon);

	//	App.Screenshot("There is a secondary toolbar menu and item");
	//}
}