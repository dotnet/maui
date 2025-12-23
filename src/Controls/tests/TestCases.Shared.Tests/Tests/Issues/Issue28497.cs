using NUnit.Framework;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.TestCases.Tests.Issues;
public class Issue28497 : _IssuesUITest
{
	public override string Issue => "[iOS] Fix Crash When Using ShellContent with DataTemplate and Binding";

	public Issue28497(TestDevice device)
	: base(device)
	{ }

	[Test]
	[Category(UITestCategories.Shell)]
	public void FlyoutItemShouldVisible()
	{
		var buttonElement = App.WaitForElement("Button");
		var buttonRect = buttonElement.GetRect();
		App.WaitForElement("Button");
		App.TapShellFlyoutIcon();
		App.WaitForElement("ShellItemLabel");
		float tapX = buttonRect.X + buttonRect.Width - 10;
		float tapY = buttonRect.CenterY();
		App.TapCoordinates(tapX, tapY);
		App.TapShellFlyoutIcon();
		App.WaitForElement("ShellItemLabel");
	}
}
