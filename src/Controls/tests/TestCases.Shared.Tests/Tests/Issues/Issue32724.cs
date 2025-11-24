# if TEST_FAILS_ON_ANDROID
using NUnit.Framework;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.TestCases.Tests.Issues;
public class Issue32724 : _IssuesUITest
{
	public Issue32724(TestDevice device) : base(device) { }

	public override string Issue => "Applying Shadow property affects the properties in Visual Transform Matrix";
	[Test, Order(1)]
	[Category(UITestCategories.Border)]
	public void VerifyScaleAndShadow()
	{
		App.WaitForElement("ScaleButton");
		App.Tap("ScaleButton");
		App.Tap("ToggleShadowButton");
		VerifyScreenshot();
	}

	[Test, Order(2)]
	[Category(UITestCategories.Border)]
	public void VerifyRotationAndShadow()
	{
		App.Tap("RotationButton");
		App.Tap("ToggleShadowButton");
		VerifyScreenshot();
	}

	[Test, Order(3)]
	[Category(UITestCategories.Border)]
	public void VerifyRotationXAndShadow()
	{
		App.Tap("RotationXButton");
		App.Tap("ToggleShadowButton");
		VerifyScreenshot();
	}

	[Test, Order(4)]
	[Category(UITestCategories.Border)]
	public void VerifyRotationYAndShadow()
	{
		App.Tap("RotationYButton");
		App.Tap("ToggleShadowButton");
		VerifyScreenshot();
	}

	[Test, Order(5)]
	[Category(UITestCategories.Border)]
	public void VerifyAnchorXAndShadow()
	{
		App.Tap("AnchorXButton");
		App.Tap("RotationButton");
		App.Tap("ToggleShadowButton");
		VerifyScreenshot();
	}
	[Test, Order(6)]
	[Category(UITestCategories.Border)]
	public void VerifyAnchorYAndShadow()
    {
        App.Tap("AnchorYButton");
		App.Tap("RotationButton");
		App.Tap("ToggleShadowButton");
		VerifyScreenshot();
    }

	[Test, Order(7)]
	[Category(UITestCategories.Border)]
	public void VerifyAnchorXAndAnchorYShadow()
	{
		App.Tap("AnchorXButton");
		App.Tap("AnchorYButton");
		App.Tap("RotationButton");
		App.Tap("ToggleShadowButton");
		VerifyScreenshot();
	}
}
#endif