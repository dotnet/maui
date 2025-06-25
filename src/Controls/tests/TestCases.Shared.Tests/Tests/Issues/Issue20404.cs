using NUnit.Framework;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.TestCases.Tests.Issues;

public class Issue20404 : _IssuesUITest
{
	public Issue20404(TestDevice testDevice) : base(testDevice) { }

	public override string Issue => "Dynamic Grid Row/Column changes don't trigger layout update on Windows until window resize";

	[Test]
	[Category(UITestCategories.Layout)]
	public void DynamicGridRowColumnChangeShouldInvalidate()
	{
		Exception? exception = null;

		App.WaitForElement("ToggleRowButton");
		App.Tap("ToggleRowButton");
		VerifyScreenshotOrSetException(ref exception, "AfterGridRowToggled");
		App.WaitForElement("ToggleColumnButton");
		App.Tap("ToggleColumnButton");
		VerifyScreenshotOrSetException(ref exception, "AfterGridColumnToggled");

		if (exception != null)
		{
			throw exception;
		}
	}
}