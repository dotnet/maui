using NUnit.Framework;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.TestCases.Tests.Issues;

public class Issue30046 : _IssuesUITest
{
	public Issue30046(TestDevice device) : base(device)
	{
	}

	public override string Issue => "[Android] Switch Shadow Does Not Follow Thumb when Toggle On or Off";

	[Test]
	[Category(UITestCategories.Switch)]
	public void SwitchShadowFollowsThumbOnToggle()
	{
		Exception? exception = null;
		App.WaitForElement("ShadowSwitch");

		// Toggle the switch to On
		App.Tap("ShadowSwitch");
		App.WaitForTextToBePresentInElement("StatusLabel", "Switch On");
		// Verify the shadow has moved with the thumb to the On position
		VerifyScreenshotOrSetException(ref exception, "SwitchShadowInOnState");

		// Toggle back to Off
		App.Tap("ShadowSwitch");
		App.WaitForTextToBePresentInElement("StatusLabel", "Switch Off");

		// Verify the shadow has returned with the thumb to the Off position
		VerifyScreenshotOrSetException(ref exception, "SwitchShadowBackInOffState");

		if (exception is not null)
		{
			throw exception;
		}
	}
}
