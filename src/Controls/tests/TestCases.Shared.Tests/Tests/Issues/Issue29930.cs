#if TEST_FAILS_ON_ANDROID // Test fails on Android , see https://github.com/dotnet/maui/issues/11812
using NUnit.Framework;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.TestCases.Tests.Issues;

public class Issue29930 : _IssuesUITest
{
	public Issue29930(TestDevice device) : base(device) { }

	public override string Issue => "[Windows] Setting a ContentView with a content of StaticResource Style Causes a System.Runtime.InteropServices.COMException.";

	[Test]
	[Category(UITestCategories.Border)]
	public void InnerContentViewShouldNotCrashWhenDynamicallyChange()
	{
		App.WaitForElement("ChangeInnerContent");
		App.Tap("ChangeInnerContent");
		App.WaitForElement("ChangeInnerContent");
	}
}
#endif