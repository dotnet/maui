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
		VerifyScreenshot();
	}
}
#endif
