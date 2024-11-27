using NUnit.Framework;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.TestCases.Tests.Issues;

public class Issue12652 : _IssuesUITest
{
	public Issue12652(TestDevice testDevice) : base(testDevice)
	{
	}

	public override string Issue => "[Bug] NullReferenceException in the Shell on UWP when navigating back to Shell Section with multiple content items";

	// [Test]
	// [Category(UITestCategories.Shell)]
	// public void NavigatingBackToAlreadySelectedTopTabDoesntCrash()
	// {
	// 	var location = App.WaitForElement("Top 3")[0];
	// 	App.TapCoordinates(location.Rect.CenterX, location.Rect.CenterY);
	// 	App.WaitForElement("TopTabPage3");
	// 	App.Tap("Main 2");
	// 	App.WaitForElement("TopTabPage2");
	// 	App.Tap("Main 1");

	// 	App.TapCoordinates(location.Rect.CenterX, location.Rect.CenterY);
	// 	App.WaitForElement("TopTabPage3");
	// 	App.Tap("Main 2");
	// 	App.WaitForElement("TopTabPage2");
	// 	App.Tap("Main 1");
	// 	App.TapCoordinates(location.Rect.CenterX, location.Rect.CenterY);
	// 	App.WaitForElement("TopTabPage3");
	// }
}