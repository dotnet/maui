using NUnit.Framework;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.TestCases.Tests.Issues;

public class Issue19690 : _IssuesUITest
{
	public Issue19690(TestDevice device) : base(device) { }

	public override string Issue => "Button VisualStates do not work";

	[Test]
	[Category(UITestCategories.Button)]
	public void ButtonVisualStatesShouldToggleBetweenNormalAndCustom()
	{
		App.WaitForElement("TestButton");

		Exception? exception = null;
		VerifyScreenshotOrSetException(ref exception, "Issue19690_InitialNormalState");

		App.Tap("TestButton");
		VerifyScreenshotOrSetException(ref exception, "Issue19690_CustomState");

		App.Tap("TestButton");
		VerifyScreenshotOrSetException(ref exception, "Issue19690_BackToNormalState");

		if (exception != null)
		{
			throw exception;
		}
	}
}
