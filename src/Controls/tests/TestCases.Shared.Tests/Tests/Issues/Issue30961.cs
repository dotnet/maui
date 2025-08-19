#if TEST_FAILS_ON_WINDOWS || TEST_FAILS_ON_CATALYST // On-screen keyboard doesn't automatically appear on desktop platforms.
using NUnit.Framework;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.TestCases.Tests.Issues;

public class Issue30961 : _IssuesUITest
{
	public override string Issue => "Setting IsTextPredictionEnabled to false for a SearchBar is not working";

	public Issue30961(TestDevice device)
	: base(device)
	{ }

	[Test]
	[Category(UITestCategories.SearchBar)]
	public void VerifySearchBarTextPrediction()
	{
		App.WaitForElement("SearchBar");
		App.EnterText("SearchBar", "apple");
		App.Tap("SearchBar");
		VerifyScreenshot();
	}
}
#endif