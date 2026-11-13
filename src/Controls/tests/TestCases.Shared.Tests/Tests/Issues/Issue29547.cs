using NUnit.Framework;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.TestCases.Tests.Issues;

public class Issue29547 : _IssuesUITest
{
	public override string Issue => "SearchBar with IsReadOnly=True still allows text deletion While pressing delete icon";

	public Issue29547(TestDevice device)
	: base(device)
	{ }

	[Test]
	[Category(UITestCategories.SearchBar)]
	public void VerifySearchBarDeleteIconBehavior()
	{
		App.WaitForElement("searchbar");
		App.TapSearchBarClearButton("searchbar");
		VerifyScreenshot();
	}
}