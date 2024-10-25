using NUnit.Framework;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.TestCases.Tests.Issues;

public class MapsModalCrash : _IssuesUITest
{
	public MapsModalCrash(TestDevice testDevice) : base(testDevice)
	{
	}

	public override string Issue => "Modal Page over Map crashes application";

	//[Test]
	//[Category(UITestCategories.Maps)]
	//[FailsOnAndroid]
	//[FailsOnIOS]
	//public void CanDisplayModalOverMap()
	//{
	//	RunningApp.WaitForElement(StartTest);
	//	RunningApp.Tap(StartTest);
	//	RunningApp.WaitForElement(DisplayModal);
	//	RunningApp.Tap(DisplayModal);
	//	RunningApp.WaitForElement(Success);
	//}
}