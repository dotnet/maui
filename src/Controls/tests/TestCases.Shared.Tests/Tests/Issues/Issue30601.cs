using NUnit.Framework;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.TestCases.Tests.Issues;

public class Issue30601 : _IssuesUITest
{
	public Issue30601(TestDevice testDevice) : base(testDevice) { }

	public override string Issue => "[Android] SearchBar does not update colors on theme change";

	[Test]
	[Category(UITestCategories.SearchBar)]
	public void SearchbarColorsShouldUpdate()
	{
		App.WaitForElement("changeThemeButton");
		App.Tap("changeThemeButton");
		VerifyScreenshot();
	}
}