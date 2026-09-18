#if ANDROID || IOS  // SafeAreaEdges not supported on Catalyst and Windows

using NUnit.Framework;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.TestCases.Tests.Issues;

public class Issue33038 : _IssuesUITest
{
	public Issue33038(TestDevice testDevice) : base(testDevice) { }

	public override string Issue => "Layout breaks on first navigation until soft keyboard appears/disappears";

	[Test]
	[Category(UITestCategories.SafeAreaEdges)]
	public void LayoutShouldBeCorrectOnFirstNavigation()
	{
		App.WaitForElement("StartPageLabel");
		App.Tap("GoToSignInButton");
		App.WaitForElement("SignInLabel");
		App.WaitForElement("EmailEntry");
		if (Device == TestDevice.Android)
		{
			// Shell navigation can surface the page content before Android has finished applying the
			// final safe-area-adjusted vertical offset on API 36, so wait for the settled bounds.
			App.RetryAssert(() =>
			{
				var signInLabel = App.WaitForElement("SignInLabel").GetRect();
				var emailEntry = App.WaitForElement("EmailEntry").GetRect();
				Assert.That(signInLabel.Y, Is.GreaterThan(200),
					"Sign-in header should be pushed below the cutout before snapshot verification.");
				Assert.That(emailEntry.Y, Is.GreaterThan(320),
					"Email entry should be pushed below the cutout before snapshot verification.");
			}, timeout: TimeSpan.FromSeconds(10));
		}

		// The layout can take an extra frame to settle after navigation, so retry the screenshot
		// comparison and allow a small tolerance for cross-machine rendering variance.
		VerifyScreenshot(tolerance: 0.5, retryTimeout: TimeSpan.FromSeconds(2));
	}
}
#endif
