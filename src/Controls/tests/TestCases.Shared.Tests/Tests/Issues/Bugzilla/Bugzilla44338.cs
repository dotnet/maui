#if TEST_FAILS_ON_CATALYST //Appium's WebDriverAgentMac only supports the "mouse" pointer type, but a "touch" action was attempted, leading to the failure.
using Xunit;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.TestCases.Tests.Issues;

[Trait("Category", UITestCategories.Cells)]
public class Bugzilla44338 : _IssuesUITest
{

	public Bugzilla44338(TestDevice testDevice) : base(testDevice)
	{
	}

	public override string Issue => "Tapping off of a cell with an open context action causes a crash in iOS 10";

	[Fact]
	public void Bugzilla44338Test()
	{
		App.WaitForElement("A");
		App.ActivateContextMenu("A");
		App.Tap("C");
	}
}
#endif