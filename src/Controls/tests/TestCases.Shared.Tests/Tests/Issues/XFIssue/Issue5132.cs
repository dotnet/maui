using NUnit.Framework;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.TestCases.Tests.Issues;

public class Issue5132 : _IssuesUITest
{
	public Issue5132(TestDevice testDevice) : base(testDevice)
	{
	}

	public override string Issue => "Unable to specify automation properties on the hamburger/flyout icon";

	//#if !(ANDROID || IOS)
	//	[Ignore("Shell test is only supported on Android and iOS")]
	//#endif
	//	[Test]
	//	[Category(UITestCategories.Shell)]
	//	public void ShellFlyoutAndHamburgerAutomationProperties()
	//	{
	//		App.WaitForElement(q => q.Marked(_idIconElement));
	//		TapInFlyout(_titleElement, _idIconElement);
	//	}
}