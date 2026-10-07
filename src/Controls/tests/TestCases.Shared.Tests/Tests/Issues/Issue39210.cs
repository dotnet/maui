#if IOS
using NUnit.Framework;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.TestCases.Tests.Issues;

public class Issue39210 : _IssuesUITest
{
	public override string Issue => "NavigationPage page view overlaps an opaque navigation bar in landscape";

	public Issue39210(TestDevice device) : base(device)
	{
	}

	[Test]
	[Category(UITestCategories.SafeAreaEdges)]
	public void PushedPageStartsBelowOpaqueNavigationBarInLandscape()
	{
		App.WaitForElement("CheckChildFrameButton");
		App.SetOrientationLandscape();
		VerifyScreenshot("PushedPageStartsBelowOpaqueNavigationBarInLandscape");
	}
}
#endif
