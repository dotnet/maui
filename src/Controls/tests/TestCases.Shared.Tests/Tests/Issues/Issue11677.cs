using NUnit.Framework;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.TestCases.Tests.Issues;
public class Issue11677 : _IssuesUITest
{
	public Issue11677(TestDevice testDevice) : base(testDevice)
	{
	}

	public override string Issue => "[iOS][maccatalyst] SearchBar BackgroundColor is black when set to transparent";

	[Test]
	[Category(UITestCategories.SearchBar)]
	public void VerifySearchBarBackground()
	{
		App.WaitForElement("Label");
		VerifyScreenshot();
	}
}