using NUnit.Framework;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.TestCases.Tests.Issues;

public class Issue25893 : _IssuesUITest
{
	public override string Issue => "Setting MenuFlyoutSubItem IconImageSource throws a NullReferenceException";

	public Issue25893(TestDevice testDevice) : base(testDevice)
	{
	}

	[Test]
	[Category(UITestCategories.ContextActions)]
	public void MenuFlyoutSubItemWithIconNoCrash()
	{
		App.WaitForElement("WaitForStubControl");

		// Without crashing, the test has passed.
	}
}