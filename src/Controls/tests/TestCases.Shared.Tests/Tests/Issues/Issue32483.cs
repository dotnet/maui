using NUnit.Framework;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.TestCases.Tests.Issues;

public class Issue32483 : _IssuesUITest
{
	public Issue32483(TestDevice device) : base(device)
	{
	}

	public override string Issue => "CursorPosition not calculated correctly on behaviors events for iOS devices";

	[Test]
	[Category(UITestCategories.Entry)]
	public void Issue32483Test()
	{
		// Tap the button to push the FlyoutPage modally
		App.WaitForElement("TestEntry");
		App.Tap("TestEntry");
		App.EnterText("TestEntry","12345");
		App.WaitForElement("CursorPosition : 5");
	}
}
