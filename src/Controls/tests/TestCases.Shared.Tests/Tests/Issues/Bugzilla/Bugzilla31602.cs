#if !WINDOWS || MACCATALYST // Setting orientation is not supported on Windows and Mac
using NUnit.Framework;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.TestCases.Tests.Issues;

public class Bugzilla31602 : _IssuesUITest
{
	public Bugzilla31602(TestDevice testDevice) : base(testDevice)
	{
	}

	public override string Issue => "not possible to programmatically open master page after iPad landscape -> portrait rotation, also tests 31664";

	// [Test]
	// [FailsOnIOSWhenRunningOnXamarinUITest]
	// [Category(UITestCategories.FlyoutPage)]
	// public void Bugzilla31602Test()
	// {
	// 	if (Devices.DeviceInfo.Idiom == Devices.DeviceIdiom.Tablet)
	// 	{
	// 		App.Tap("Sidemenu Opener");
	// 		App.WaitForElement("SideMenu");
	// 		App.SetOrientationLandscape();
	// 		App.WaitForElement("SideMenu");
	// 		App.SetOrientationPortrait();
	// 		App.WaitForNoElement("SideMenu");
	// 		App.Tap("Sidemenu Opener");
	// 		App.WaitForElement("SideMenu");
	// 	}
	// }

	// [TearDown]
	// public void TearDown()
	// {
	// 	App.SetOrientationPortrait();
	// }
}
#endif