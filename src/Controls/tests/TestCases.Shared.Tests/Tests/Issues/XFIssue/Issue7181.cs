#if TEST_FAILS_ON_IOS && TEST_FAILS_ON_CATALYST // On iOS and Catalyst, the ToolbarItem text does not update in the Accessibility layer. The label value in the accessibility information remains unchanged from the initial value.
using NUnit.Framework;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.TestCases.Tests.Issues;

public class Issue7181 : _IssuesUITest
{
	const string ToolbarBtn = "Toolbar button";
	const string DefaultToolbarItemText = "Toolbar test";
	const string AfterClickToolbarItemText = "Button Clicked";
	const string SetToolbarIconBtn = "Set toolbar icon button";

	public Issue7181(TestDevice testDevice) : base(testDevice)
	{
	}

	public override string Issue => "[Bug] Cannot update ToolbarItem text and icon";

	[Test]
	[Category(UITestCategories.Shell)]
	public void ShellToolbarItemTests()
	{
		var count = 0;

		App.WaitForElement(DefaultToolbarItemText);

		for (int i = 0; i < 5; i++)
		{
			if (i == 0)
				App.Tap(DefaultToolbarItemText);
			else
				App.Tap($"{AfterClickToolbarItemText} {count++}");

			App.WaitForElement($"{AfterClickToolbarItemText} {count}");

		}

		App.Tap("Click to change toolbarIcon");
	}
}
#endif