#if TEST_FAILS_ON_CATALYST || TEST_FAILS_ON_IOS // There's a separate PR for this issue on iOS and Catalyst https://github.com/dotnet/maui/pull/30597
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
#endif