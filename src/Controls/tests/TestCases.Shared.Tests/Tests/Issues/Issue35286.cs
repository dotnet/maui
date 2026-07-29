using NUnit.Framework;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.TestCases.Tests.Issues;

public class Issue35286 : _IssuesUITest
{
	public Issue35286(TestDevice device) : base(device) { }

	public override string Issue => "Spacing problem with SearchBar";

	[Test]
	[Category(UITestCategories.SearchBar)]
	public void VerifySearchBarHeightRequestValues()
	{
		App.WaitForElement("Issue35286Label1");
        VerifyScreenshot();
	}
}
