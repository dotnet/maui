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
		var scale = App is AppiumAndroidApp ? App.GetDisplayDensity() : 1;
		App.RetryAssert(() =>
		{
			var layout = App.WaitForElementAndGetRect("SignInLayout");
			var label = App.WaitForElementAndGetRect("SignInLabel");
			var entry = App.WaitForElementAndGetRect("EmailEntry");
			Assert.That(layout.Top, Is.GreaterThan(0), "The first layout must respect the top safe area.");
			Assert.That(label.Height, Is.GreaterThan(0));
			Assert.That(entry.Height, Is.GreaterThan(0));
			Assert.That(label.Top, Is.EqualTo(layout.Top + 20 * scale).Within(2));
			Assert.That(label.Left, Is.EqualTo(layout.Left + 20 * scale).Within(2));
			Assert.That(entry.Left, Is.EqualTo(label.Left).Within(1));
			Assert.That(entry.Right, Is.EqualTo(layout.Right - 20 * scale).Within(2));
			Assert.That(entry.Top, Is.EqualTo(label.Bottom + 16 * scale).Within(2));
			Assert.That(entry.Bottom, Is.LessThanOrEqualTo(layout.Bottom));
		});
		Assert.That(App.IsKeyboardShown(), Is.False, "Layout must be correct before opening the keyboard.");
	}
}
#endif
