using NUnit.Framework;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.TestCases.Tests.Issues;

public class Issue27732 : _IssuesUITest
{
	public Issue27732(TestDevice testDevice) : base(testDevice)
	{
	}

	public override string Issue => "[Windows] View Position Shifts When Shadows Are Dynamically Removed or Resized";

	const string ToggleShadowButton = "ToggleShadowButton";

	[Test]
	[Category(UITestCategories.Visual)]
	public void ViewShouldNotShiftOnShadowChanged()
	{
		App.WaitForElement(ToggleShadowButton);
		for (int i = 0; i < 5; i++)
		{
			App.Tap(ToggleShadowButton);
		}
		VerifyScreenshot();
	}
}