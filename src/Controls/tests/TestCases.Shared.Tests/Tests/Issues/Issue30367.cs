using NUnit.Framework;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.TestCases.Tests.Issues;

public class Issue30367 : _IssuesUITest
{
	public override string Issue => "SearchBar FlowDirection Property Not Working on Android";

	public Issue30367(TestDevice device)
	: base(device)
	{ }

	[Test]
	[Category(UITestCategories.SearchBar)]
	public void VerifySearchBarFlowDirection()
	{
		App.WaitForElement("SearchBarLabel");
		VerifyScreenshot();
	}
}