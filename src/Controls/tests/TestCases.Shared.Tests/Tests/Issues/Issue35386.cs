#if IOS || ANDROID // SoftAreaEdges is only available on mobile platforms
using NUnit.Framework;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.TestCases.Tests.Issues;

public class Issue35386 : _IssuesUITest
{
	public Issue35386(TestDevice device) : base(device) { }

	public override string Issue => "MauiView leaks detached platform views when SafeAreaEdges includes SoftInput";

	[Test]
	[Category(UITestCategories.SafeAreaEdges)]
	public void SoftInputSafeArea_DetachedPlatformViews_DoNotLeak()
	{
		App.WaitForElement("statusLabel");
		Assert.That(
			App.WaitForTextToBePresentInElement("statusLabel", "Suspect SafeAreaEdges.SoftInput: virtual=0/12, handler=0/12, platform=0/12", timeout: TimeSpan.FromSeconds(25)),
			Is.True,
			"The status label did not reach the expected SoftInput summary text.");
	}
}
#endif
