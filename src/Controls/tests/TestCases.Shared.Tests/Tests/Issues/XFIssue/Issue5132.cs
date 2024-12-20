//#if TEST_FAILS_ON_WINDOWS // Can't able to override the automation id property for Android, as it always with default value,  Also in Windows we can't override the icon. More Information: https://github.com/dotnet/maui/issues/1625
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
		App.WaitForElement(AppiumQuery.ByAccessibilityId(_idIconElement));
		App.Tap(AppiumQuery.ByAccessibilityId(_idIconElement));
		App.WaitForElement("Connect");
		 
	}
}