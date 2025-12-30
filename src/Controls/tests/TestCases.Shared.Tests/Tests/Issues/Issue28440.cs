using NUnit.Framework;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.TestCases.Tests.Issues;

public class Issue28440 : _IssuesUITest
{
	public override string Issue => "FlyoutPage IsPresented not updated properly in windows";

	public Issue28440(TestDevice device)
	: base(device)
	{ }

	[Test]
	[Category(UITestCategories.FlyoutPage)]
	public void CheckFlyoutPageIsPresentedState()
	{
		var buttonElement = App.WaitForElement("Button");
		var buttonRect = buttonElement.GetRect();
		App.Tap("Button");
		App.WaitForElement("flyoutMenu");
		float tapX = buttonRect.X + buttonRect.Width - 10;
		float tapY = buttonRect.CenterY();
		App.TapCoordinates(tapX, tapY);
		App.WaitForElement("Button");
		App.Tap("Button");
		App.WaitForElement("flyoutMenu");
	}
}
