using NUnit.Framework;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.TestCases.Tests.Issues;

public class Issue35257 : _IssuesUITest
{
	public Issue35257(TestDevice device)
		: base(device)
	{
	}

	public override string Issue => "iOS 26 Switch initial Off/On/Thumb colors are incorrect until toggled";

	[Test]
	[Category(UITestCategories.Switch)]
	public void VerifySwitchInitialAndPostToggleColors()
	{
		Exception? exception = null;

		App.WaitForElement("CycleSwitchStatesButton");

		VerifyScreenshotOrSetException(ref exception, "Issue35257_Initial");

		App.Tap("CycleSwitchStatesButton");
		VerifyScreenshotOrSetException(ref exception, "Issue35257_Toggled");

		App.Tap("CycleSwitchStatesButton");
		VerifyScreenshotOrSetException(ref exception, "Issue35257_BackToInitial");

		if (exception is not null)
		{
			throw exception;
		}
	}
}
