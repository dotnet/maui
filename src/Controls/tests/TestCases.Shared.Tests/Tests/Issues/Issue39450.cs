using NUnit.Framework;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.TestCases.Tests.Issues;

public class Issue39450 : _IssuesUITest
{
	public Issue39450(TestDevice device) : base(device) { }

	public override string Issue => "Button gradient background should respect CornerRadius";

	[Test]
	[Category(UITestCategories.Button)]
	public void GradientBackgroundsShouldRespectCornerRadiusAcrossVisualStates()
	{
		App.WaitForElement("ToggleGradientStateButton");

		Exception? exception = null;
		VerifyScreenshotOrSetException(ref exception, "Issue39450_InitialBackgrounds", retryTimeout: TimeSpan.FromSeconds(2));

		App.Tap("ToggleGradientStateButton");
		Assert.That(App.WaitForTextToBePresentInElement("StateLabel", "Gradient"), Is.True);
		VerifyScreenshotOrSetException(ref exception, "Issue39450_GradientVisualState", retryTimeout: TimeSpan.FromSeconds(2));

		App.Tap("ToggleGradientStateButton");
		Assert.That(App.WaitForTextToBePresentInElement("StateLabel", "Normal"), Is.True);
		VerifyScreenshotOrSetException(ref exception, "Issue39450_RestoredBackground", retryTimeout: TimeSpan.FromSeconds(2));

		if (exception is not null)
		{
			throw exception;
		}
	}
}
