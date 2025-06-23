#if TEST_FAILS_ON_WINDOWS && TEST_FAILS_ON_CATALYST // Setting orientation is not supported on Windows and mac
using NUnit.Framework;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.TestCases.Tests.Issues;

public class Bugzilla30353 : _IssuesUITest
{
	const string DetailToggle = "DetailToggle";
	const string FlyoutToggle = "FlyoutToggle";
	const string FlyoutVisibleText = "The Flyout is now visible";
	const string FlyoutInvisibleText = "The Flyout is now invisible";

	public Bugzilla30353(TestDevice testDevice) : base(testDevice)
	{
	}

	public override string Issue => "FlyoutPage.IsPresentedChanged is not raised";
	[Test]
	[Category(UITestCategories.FlyoutPage)]
	public void FlyoutPageIsPresentedChangedRaised()
	{
		App.SetOrientationPortrait();
		App.WaitForElement(DetailToggle);
		App.Tap(DetailToggle);
		App.WaitForElement(FlyoutVisibleText);
		App.Tap(FlyoutToggle);
		App.WaitForElement(FlyoutInvisibleText);
		App.SetOrientationLandscape();
		App.WaitForElement(FlyoutInvisibleText);
		App.Tap(DetailToggle);
		App.WaitForElement(FlyoutVisibleText);
		App.Tap(FlyoutToggle);
		App.WaitForElement(FlyoutInvisibleText);
		App.SetOrientationPortrait();
		App.Tap(DetailToggle);
		App.WaitForElement(FlyoutVisibleText);
		App.Tap(FlyoutToggle);
		App.WaitForElement(FlyoutInvisibleText);
		App.SetOrientationLandscape();
	}

	[TearDown]
	public void TearDown()
	{
		App.SetOrientationPortrait();
	}
}
#endif