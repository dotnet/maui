#if TEST_FAILS_ON_WINDOWS // test fails on windows , see https://github.com/dotnet/maui/issues/29930
using NUnit.Framework;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.TestCases.Tests.Issues;

public class Issue11812 : _IssuesUITest
{
	public Issue11812(TestDevice device) : base(device) { }

	public override string Issue => "Setting Content of ContentView through style would crash on parent change";

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