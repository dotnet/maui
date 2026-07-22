using NUnit.Framework;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.TestCases.Tests.Issues;

public class Issue19883 : _IssuesUITest
{
	public override string Issue => "Switch OnColor not applied correctly and ThumbColor not reset when toggled off";

	public Issue19883(TestDevice device) : base(device) { }

	[Test]
	[Category(UITestCategories.Switch)]
	public void SettingThumbColorToNullResetsToDefault()
	{
		App.WaitForElement("TestSwitch");

		// Initial state: ThumbColor is Orange, no OnColor set
		VerifyScreenshot("Issue19883_ThumbColorOrange");

		// Reset ThumbColor to null: switch should revert to the default thumb color
		App.Tap("ResetThumbColorButton");
		VerifyScreenshot("Issue19883_ThumbColorNull");
	}
}
