#if TEST_FAILS_ON_IOS && TEST_FAILS_ON_CATALYST //ContextActions Menu Items Not Accessible via Automation on iOS and Catalyst Platforms. 
//For more information see Issue Link: https://github.com/dotnet/maui/issues/27394
using NUnit.Framework;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.TestCases.Tests.Issues;

public class Bugzilla31330 : _IssuesUITest
{
	public Bugzilla31330(TestDevice testDevice) : base(testDevice)
	{
	}

	public override string Issue => "Disabled context actions appear enabled";

	[Test]
	[Category(UITestCategories.ListView)]
	public void Bugzilla31330Test()
	{
		App.WaitForElement("Something 2");
		App.ActivateContextMenu("Something 1");
		App.WaitForElement("Delete");
		App.Tap("Delete");
		App.DismissContextMenu();
		App.Tap("Something 2");
		App.ActivateContextMenu("Something 2");
		App.WaitForElement("Delete");
		App.Tap("Delete");
		App.WaitForNoElement("Something 2");
	}
}
#endif