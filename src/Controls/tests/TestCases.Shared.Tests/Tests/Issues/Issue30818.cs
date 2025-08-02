#if TEST_FAILS_ON_WINDOWS // It is not implemented on Windows yet
using NUnit.Framework;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.TestCases.Tests.Issues;

public class Issue30818 : _IssuesUITest
{
	public Issue30818(TestDevice device) : base(device) { }

	public override string Issue => "ToolbarItem - Icon Color property";

	[Test]
	[Category(UITestCategories.ToolbarItem)]
	public void ToolbarItemIconColorShouldBeApplied()
	{
		App.WaitForElement("Label");
		VerifyScreenshot();
	}
}
#endif