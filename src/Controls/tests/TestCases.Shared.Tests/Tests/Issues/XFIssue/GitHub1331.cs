#if TEST_FAILS_ON_IOS && TEST_FAILS_ON_CATALYST //ContextActions Menu Items Not Accessible via Automation on iOS and Catalyst Platforms. 
//For more information see Issue Link: https://github.com/dotnet/maui/issues/27394
using NUnit.Framework;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.TestCases.Tests.Issues;

public class GitHub1331 : _IssuesUITest
{
	const string Action = "Action 1";
	const string ActionItemTapped = "Action Item Tapped";
	const string CellItem = "item 1";

	public GitHub1331(TestDevice testDevice) : base(testDevice)
	{
	}

	public override string Issue => "[Android] ViewCell shows ContextActions on tap instead of long press";

	[Test]
	[Category(UITestCategories.Gestures)]
	public void SingleTapOnCellDoesNotActivateContext()
	{
		App.WaitForElement(Action);

		App.Tap(Action);
		App.WaitForElement(ActionItemTapped);

		// Tapping the part of the cell without a tap gesture should *not* display the context action
		App.Tap(CellItem);
		App.WaitForNoElement("Context Action");

		// But a Long Press *should* still display the context action
		App.ActivateContextMenu(CellItem);
		App.WaitForElement("Context Action");
	}
}
#endif
