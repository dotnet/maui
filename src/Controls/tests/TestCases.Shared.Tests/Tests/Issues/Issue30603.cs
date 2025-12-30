using NUnit.Framework;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.TestCases.Tests.Issues;

public class Issue30603 : _IssuesUITest
{
	public Issue30603(TestDevice testDevice) : base(testDevice) { }

	public override string Issue => "[Android] Editor and Entry don't update placeholder and text color on theme change";

	[Test]
	[Category(UITestCategories.Editor)]
	[Category(UITestCategories.Entry)]
	public void EditorAndEntryInputFieldsShouldChangeColorsOnAppThemeChange()
	{
		App.WaitForElement("changeThemeButton");
		App.Tap("changeThemeButton");
		VerifyScreenshot();
	}
}