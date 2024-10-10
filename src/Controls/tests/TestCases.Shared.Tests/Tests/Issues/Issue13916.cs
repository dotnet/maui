using NUnit.Framework;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.TestCases.Tests.Issues;

public class Issue13916 : _IssuesUITest
{
	public Issue13916(TestDevice testDevice) : base(testDevice)
	{
	}

	public override string Issue => "[iOS] iOS Application crashes on Back press when navigated to using GoToAsync with \"//\" or \"///\" route if 2 or more things are removed from the navigation stack";

	// [Test]
	// [Category(UITestCategories.Shell)]
	// public void RemovingMoreThanOneInnerPageAndThenPushingAPageCrashes()
	// {
	// 	App.Tap("ClickMe1");
	// 	App.Tap("ClickMe2");
	// 	App.Tap("ClickMe3");
	// 	App.WaitForElement("Success");
	// }
}
