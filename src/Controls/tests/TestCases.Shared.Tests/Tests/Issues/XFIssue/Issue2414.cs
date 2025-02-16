#if TEST_FAILS_ON_IOS && TEST_FAILS_ON_CATALYST //ContextActions Menu Items Not Accessible via Automation on iOS and Catalyst Platforms. 
//For more information see Issue Link: https://github.com/dotnet/maui/issues/27394
using NUnit.Framework;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.TestCases.Tests.Issues;

public class Issue2414 : _IssuesUITest
{
	public Issue2414(TestDevice testDevice) : base(testDevice)
	{
	}

	public override string Issue => "NullReferenceException when swiping over Context Actions";

	[Test]
	[Category(UITestCategories.TableView)]
	public void TestShowContextMenuItemsInTheRightOrder()
	{
		App.WaitForElement("Swipe ME");
		App.ActivateContextMenu("Swipe ME");
		App.WaitForElement("Text0");
		App.Tap("Text0");
		App.WaitForElement("Swipe ME");
	}
}
#endif