using NUnit.Framework;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.TestCases.Tests.Issues;

public class Issue14164 : _IssuesUITest
{
	public Issue14164(TestDevice device) : base(device) { }

	public override string Issue => "[iOS, MAC] Border overlapped with content view label when setting clip";

	[Test, Order(1)]
	[Category(UITestCategories.Border)]
	public void BorderContentShouldNotDisappearWhenClippingApplied()
	{
		App.WaitForElement("AddClipButton");
		App.Tap("AddClipButton");
		VerifyScreenshot();
	}

	[Test, Order(2)]
	[Category(UITestCategories.Border)]
	public void BorderContentShouldNotDisappearWhenClippingRemoved()
	{
		App.WaitForElement("RemoveClipButton");
		App.Tap("RemoveClipButton");
		VerifyScreenshot();
	}
}