#if TEST_FAILS_ON_IOS && TEST_FAILS_ON_CATALYST //ContextActions Menu Items Not Accessible via Automation on iOS and Catalyst Platforms. 
//For more information see Issue Link: https://github.com/dotnet/maui/issues/27394
using NUnit.Framework;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.TestCases.Tests.Issues;

public class Bugzilla42832 : _IssuesUITest
{
	public Bugzilla42832(TestDevice testDevice) : base(testDevice)
	{
	}

	public override string Issue => "Scrolling a ListView with active ContextAction Items causes NRE";

	[Test]
	[Category(UITestCategories.ListView)]
	public void ContextActionsScrollNRE()
	{
		App.WaitForElement("Item #0");
		App.ActivateContextMenu("Item #0");
		App.WaitForElement("Test Item");

		int counter = 0;
		while (counter < 5)
		{
			App.ScrollDown("Item #10", ScrollStrategy.Gesture);
			App.ScrollUp("Item #0", ScrollStrategy.Gesture);
			counter++;
		}
	}
}
#endif