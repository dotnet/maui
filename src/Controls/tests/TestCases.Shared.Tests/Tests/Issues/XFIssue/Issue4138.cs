using NUnit.Framework;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.TestCases.Tests.Issues;

public class Issue4138 : _IssuesUITest
{
	public Issue4138(TestDevice testDevice) : base(testDevice)
	{
	}

	public override string Issue => "[iOS] NavigationPage.TitleIcon no longer centered";

	//[Test]
	//[Category(UITestCategories.Navigation)]
	//[FailsOnIOS]
	//public void TitleIconIsCentered()
	//{
	//	var element = App.WaitForElement("coffee.png")[0];
	//	var rect = App.RootViewRect();
	//	Assert.AreEqual(element.Rect.CenterX, rect.CenterX);
	//}
}