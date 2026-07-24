#if ANDROID // Android-only: #36269 is a regression in Android's safe-area inset recompute path.
using NUnit.Framework;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.TestCases.Tests.Issues;

public class Issue36269 : _IssuesUITest
{
	public Issue36269(TestDevice device) : base(device) { }

	public override string Issue => "SafeAreaEdges bottom padding is lost when the Shell TabBar is hidden at runtime on a page that started above the navigation bar";

	[Test]
	[Category(UITestCategories.SafeAreaEdges)]
	public void SafeAreaBottomPaddingIsAppliedWhenTabBarHiddenAtRuntime()
	{
		App.WaitForElement("HideTabBarButton");
		App.Tap("HideTabBarButton");

		App.WaitForElement("BottomEdgeLabel");

		VerifyScreenshot(retryTimeout: TimeSpan.FromSeconds(2));
	}
}
#endif
