using NUnit.Framework;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.TestCases.Tests.Issues;

public class Issue30597 : _IssuesUITest
{
	public Issue30597(TestDevice testDevice) : base(testDevice) { }

	public override string Issue => "[iOS] SearchBar placeholder color is not updating on theme change";

	[Test]
	[Category(UITestCategories.SearchBar)]
	public void PlaceholderColorShouldChange()
	{
		App.WaitForElement("changeThemeButton");
		App.Tap("changeThemeButton");
		VerifyScreenshot();
	}
}