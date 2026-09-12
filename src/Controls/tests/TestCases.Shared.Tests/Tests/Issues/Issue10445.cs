using NUnit.Framework;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.TestCases.Tests.Issues;

public class Issue10445 : _IssuesUITest
{
	public Issue10445(TestDevice device) : base(device)
	{
	}

	public override string Issue => "Shell.Background does not support gradient brushes";

	[Test]
	[Category(UITestCategories.Shell)]
	public void ShellBackgroundSupportsGradientBrush()
	{
		// Wait for the page to be fully loaded
		App.WaitForElement("GradientInfoLabel");

		// Verify the Shell renders correctly with a gradient background
		VerifyScreenshot();
	}

	[Test]
	[Category(UITestCategories.Shell)]
	public void ShellBackgroundTransitionsFromGradientToSolidAndDefault()
	{
		Exception? exception = null;
		App.WaitForElement("GradientInfoLabel");
		App.Tap("GradientInfoLabel");
		App.WaitForElement("Shell.Background should display solid blue in the navigation and tab bars.");
		VerifyScreenshotOrSetException(ref exception, "ShellBackgroundChangesFromGradientToSolid", retryTimeout: TimeSpan.FromSeconds(2));

		App.Tap("GradientInfoLabel");
		App.WaitForElement("Shell.Background should display the platform default in the navigation and tab bars.");
		VerifyScreenshotOrSetException(ref exception, "ShellBackgroundResetsToDefault", retryTimeout: TimeSpan.FromSeconds(2));

		if (exception is not null)
		{
			throw exception;
		}
	}
}
