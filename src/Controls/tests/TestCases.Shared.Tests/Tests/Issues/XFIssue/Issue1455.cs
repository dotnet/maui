#if ANDROID // <!-- This test case applies only to Android due to the use of android-specific configurations
using NUnit.Framework;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.TestCases.Tests.Issues;

public class Issue1455 : _IssuesUITest
{
	public Issue1455(TestDevice testDevice) : base(testDevice)
	{
	}

	public override string Issue => "Context action are not changed when selected item changed on Android";

	[Test]
	[Category(UITestCategories.ListView)]
	public void RefreshContextActions()
	{
		App.WaitForElement("Cell 1");
		App.ActivateContextMenu("Cell 4");
		App.WaitForElement("Vestibulum");
		App.Tap("Cell 5");
		App.WaitForElement("Hendrerit");
		App.Back();
		App.WaitForElement("Toggle LegacyMode");
		App.Tap("Toggle LegacyMode");
		App.ActivateContextMenu("Cell 4");
		App.WaitForElement("Vestibulum");
		App.Tap("Cell 5");
		App.WaitForElement("Vestibulum");
	}
}
#endif