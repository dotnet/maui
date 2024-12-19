#if TEST_FAILS_ON_ANDROID && TEST_FAILS_ON_WINDOWS // AutomationId not working on Android and Icon not override on Windows More Information: https://github.com/dotnet/maui/issues/1625
using NUnit.Framework;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.TestCases.Tests.Issues;

public class Issue5132 : _IssuesUITest
{
	public Issue5132(TestDevice testDevice) : base(testDevice)
	{
	}
	const string _idIconElement = "shellIcon";
	public override string Issue => "Unable to specify automation properties on the hamburger/flyout icon";

	
	[Test]
	[Category(UITestCategories.Shell)]
	public void ShellFlyoutAndHamburgerAutomationProperties()
	{
		App.WaitForElement(_idIconElement);
		App.Tap(_idIconElement);
		App.WaitForElement("Connect");
		 
	}
}
#endif