#if TEST_FAILS_ON_WINDOWS && TEST_FAILS_ON_CATALYST // Orientation not support in Windows and Catalyst.
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


	[Test]
	[Category(UITestCategories.FlyoutPage)]
	public void Bugzilla34632Test()
	{
		App.WaitForElement("btnModal");
		App.SetOrientationPortrait();
		App.WaitForElement("btnModal");
		App.Tap("btnModal");
		App.SetOrientationLandscape();
		App.WaitForElement("btnDismissModal");
		App.Tap("btnDismissModal");
		App.WaitForElement("btnModal");
		App.Tap("btnModal");
		App.SetOrientationPortrait();
		Thread.Sleep(1000);
		App.WaitForElement("btnDismissModal");
		App.Tap("btnDismissModal");
		App.TapFlyoutPageIcon("Main Page");
		App.WaitForElement("btnFlyout");
		App.Tap("btnFlyout");
		App.WaitForNoElement("btnFlyout");
	}

	[TearDown]
	public void TearDown()
	{
		App.SetOrientationPortrait();
	}
}
#endif