using NUnit.Framework;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.TestCases.Tests.Issues;

public class Issue32419 : _IssuesUITest
{
	public override string Issue => "[iOS, macOS] Shell menu and flyout items do not update correctly in RTL mode";

	public Issue32419(TestDevice device)
	: base(device)
	{ }

	[Test, Order(1)]
	[Category(UITestCategories.Shell)]
	public void VerifyShellFlyoutContentAlignedInRTL()
	{
		App.WaitForElement("homePageLabel");
		VerifyScreenshot();
	}

	[Test, Order(2)]
	[Category(UITestCategories.Shell)]
	public void VerifyShellMenuItemsAlignedInRTL()
	{
		if (_testDevice == TestDevice.Android && string.Equals(
			Environment.GetEnvironmentVariable("TEST_CONFIGURATION_ARGS"),
			"ShellRenderer", StringComparison.OrdinalIgnoreCase))
		{
			Assert.Ignore("Fails with Android ShellRenderer: https://github.com/dotnet/maui/issues/39461");
		}

		App.WaitForElement("homePageLabel");
		App.Tap("FlyoutLockedButton");
		App.WaitForElement("MenuItem 1");
		VerifyScreenshot(retryTimeout: TimeSpan.FromSeconds(2));
	}

}
