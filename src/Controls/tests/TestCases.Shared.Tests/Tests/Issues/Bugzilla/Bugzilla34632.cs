#if !IOS && !WINDOWS // Setting orientation is not supported on Windows
using NUnit.Framework;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.TestCases.Tests.Issues;

public class Bugzilla34632 : _IssuesUITest
{
	public Bugzilla34632(TestDevice testDevice) : base(testDevice)
	{
	}

	public override string Issue => "Can't change IsPresented when setting SplitOnLandscape ";

	// [Test]
	// [Category(UITestCategories.FlyoutPage)]
	// public void Bugzilla34632Test()
	// {
	// 	if (Devices.DeviceInfo.Idiom == Devices.DeviceIdiom.Tablet)
	// 	{
	// 		App.SetOrientationPortrait();
	// 		App.Tap("btnModal");
	// 		App.SetOrientationLandscape();
	// 		App.Tap("btnDismissModal");
	// 		App.Tap("btnModal");
	// 		App.SetOrientationPortrait();
	// 		App.Tap("btnDismissModal");
	// 		App.Tap("Main Page");
	// 		App.Tap("btnFlyout");
	// 		App.WaitForNoElement("btnFlyout");
	// 	}
	// 	else
	// 	{
	// 		// Wait for the test to finish loading before exiting otherwise
	// 		// the next UI test might start running while this is still loading
	// 		App.WaitForElement("btnModal");
	// 	}
	// }

	// [TearDown]
	// public void TearDown()
	// {
	// 	App.SetOrientationPortrait();
	// }
}
#endif