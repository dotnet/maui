// Crash is Android-specific: RenderThread GL functor receives zero-area Skia canvas when ClipBounds=(0,0,0,0) at (w>0,h=0)
#if ANDROID
using NUnit.Framework;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.TestCases.Tests.Issues;

public class Issue35771 : _IssuesUITest
{
	public Issue35771(TestDevice device) : base(device) { }

	public override string Issue => "Android SIGSEGV crash with multiple auto-sizing WebViews in ScrollView on navigated page";

	[Test]
	[Category(UITestCategories.WebView)]
	public void MultipleAutoSizingWebViewsInScrollViewShouldNotCrash()
	{
		App.WaitForElement("Issue35771NavigateButton");
		App.Tap("Issue35771NavigateButton");
		App.WaitForElement("Issue35771Ready");
		App.Back();
		App.WaitForElement("Issue35771NavigateButton");
	}

	[Test]
	[Category(UITestCategories.WebView)]
	public void HorizontalAutoSizingWebViewsShouldNotCrash()
	{
		App.WaitForElement("Issue35771HorizontalNavigateButton");
		App.Tap("Issue35771HorizontalNavigateButton");
		App.WaitForElement("Issue35771HorizontalReady");
		App.Back();
		App.WaitForElement("Issue35771NavigateButton");
	}

	[Test]
	[Category(UITestCategories.WebView)]
	public void PopAsyncFromAutoSizingWebViewPageShouldNotCrash()
	{
		App.WaitForElement("Issue35771PopAsyncNavigateButton");
		App.Tap("Issue35771PopAsyncNavigateButton");
		App.WaitForElement("Issue35771PopAsyncReady");
		App.Tap("Issue35771PopAsyncPopButton");
		App.WaitForElement("Issue35771NavigateButton");
	}
}
#endif
