using NUnit.Framework;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.TestCases.Tests.Issues;

public class Issue4720 : _IssuesUITest
{
	public Issue4720(TestDevice testDevice) : base(testDevice)
	{
	}

	public override string Issue => "UWP: Webview: Memory Leak in WebView";

	//[Test]
	//[Category(UITestCategories.WebView)]
	//[FailsOnIOS]
	//public void WebViewDoesntCrashWhenLoadingAHeavyPageAndUsingExecutionModeSeparateProcess()
	//{
	//	//4 iterations were enough to run out of memory before the fix.
	//	int iterations = 10;

	//	for (int n = 0; n < iterations; n++)
	//	{
	//		App.WaitForElement(q => q.Marked("New Page"));
	//		App.Tap(q => q.Marked("New Page"));
	//		App.WaitForElement(q => q.Marked("Close Page"));
	//		Thread.Sleep(250);
	//		App.Tap(q => q.Marked("Close Page"));
	//	}
	//	App.Tap(q => q.Marked("GC"));
	//}
}