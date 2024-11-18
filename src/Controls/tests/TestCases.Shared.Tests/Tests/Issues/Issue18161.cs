using NUnit.Framework;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.TestCases.Tests.Issues;

public class Issue18161 : _IssuesUITest
{
	public Issue18161(TestDevice device)
		: base(device)
	{ }

	public override string Issue => "Toggling FlyoutLayoutBehavior on Android causes the app to crash";

	[Test]
	[Category(UITestCategories.FlyoutPage)]
	public void NoExceptionShouldBeThrown()
	{
		App.WaitForElement("ToggleBehaviour");
		App.Tap("ToggleBehaviour");
		App.Tap("ToggleBehaviour");

		//The test passes if no exception is thrown
	}
}