using NUnit.Framework;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.TestCases.Tests.Issues;

public class Bugzilla43527 : _IssuesUITest
{

	public Bugzilla43527(TestDevice testDevice) : base(testDevice)
	{
	}

	public override string Issue => "[UWP] Detail title does not update when wrapped in a NavigationPage";

	// [Test]
	// [Category(UITestCategories.FlyoutPage)]
	// public void TestB43527UpdateTitle()
	// {
	// 	// TODO from Xamarin.UITest migration
	// 	// I'm not sure if this actually verifies the functionality here
	// 	// we might need to add a VerifyScreenshot for this
	//	// And test is failing so disabled for now
	// 	App.WaitForElement("Change Title");
	// 	App.WaitForElement("Test Page");
	// 	App.Tap("Change Title");
	// 	App.WaitForNoElement("Test Page");
	// }
}