using NUnit.Framework;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.TestCases.Tests.Issues;

public class Issue34296 : _IssuesUITest
{
	public override string Issue => "TapGestureRecognizer on GraphicsView causes a crash on Android devices";

	public Issue34296(TestDevice device) : base(device)
	{
	}

	[Test]
	[Category(UITestCategories.GraphicsView)]
	public void TapGestureOnGraphicsViewShouldNotCrash()
	{
		App.WaitForElement("Issue34296StatusLabel");
		App.WaitForElement("Issue34296GraphicsView");

		App.Tap("Issue34296GraphicsView");
		App.WaitForElement("Issue34296StatusLabel");

		App.Tap("Issue34296GraphicsView");

		var statusText = App.FindElement("Issue34296StatusLabel").GetText();
		Assert.That(statusText, Is.EqualTo("TapCount:2"));
	}
}
