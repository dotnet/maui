#if IOS //This test case verifies "UIButton CurrentImage can be set without crashing" in iOS platform only.
using Xunit;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.TestCases.Tests.Issues;

public class Issue25409 : _IssuesUITest
{
	public Issue25409(TestDevice testDevice) : base(testDevice)
	{
	}

	public override string Issue => "UIButton CurrentImage can be set without crashing";

	[Fact]
	[Trait("Category", UITestCategories.Button)]
	public void CurrentImageSet()
	{
		App.WaitForElement("button1");
		App.Tap("button1");
		App.WaitForElement("button1");
		App.Tap("button1");
		App.WaitForElement("button1");
		App.Tap("button1");
	}
}
#endif
