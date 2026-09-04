// Crash is Android-specific: RenderThread GL functor SIGSEGVs when a non-null ClipBounds
// routes an off-screen WebView's compositing through GLFunctorDrawable on overscroll.
#if ANDROID
using NUnit.Framework;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.TestCases.Tests.Issues;

public class Issue38080 : _IssuesUITest
{
	public Issue38080(TestDevice device) : base(device) { }

	public override string Issue => "Android SIGSEGV crash in GLFunctorDrawable when a ScrollView with an off-screen WebView is overscrolled";

	[Test]
	[Category(UITestCategories.WebView)]
	public void OffScreenWebViewInScrollViewShouldNotCrashOnOverscroll()
	{
		App.WaitForElement("Issue38080NavigateButton");
		App.Tap("Issue38080NavigateButton");
		App.WaitForElement("Issue38080Ready");

		// Fling to the top extreme: the WebView is off-screen below the fold and the
		// ScrollView overscrolls past its top edge.
		for (int i = 0; i < 6; i++)
			App.ScrollDown("Issue38080ScrollView", ScrollStrategy.Gesture, 0.9, 100);

		// Fling to the bottom extreme: the WebView is off-screen above the fold and the
		// ScrollView overscrolls past its bottom edge.
		for (int i = 0; i < 15; i++)
			App.ScrollUp("Issue38080ScrollView", ScrollStrategy.Gesture, 0.9, 100);

		// The WebView is now off-screen at the bottom; back navigation also reproduced the
		// RenderThread crash while compositing. Assert we returned to the home page alive.
		App.Back();
		App.WaitForElement("Issue38080NavigateButton");
	}
}
#endif
