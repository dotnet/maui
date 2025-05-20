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
		var view = App.WaitForElement("searchbar");

#if !WINDOWS
		App.TapSearchBarClearButton();
#else
		var rect = view.GetRect();
		App.TapCoordinates(rect.Width / 2, rect.Y + rect.Height / 2);
		App.TapCoordinates(rect.Right - 84, rect.Y + rect.Height / 2);
#endif
		VerifyScreenshot();
	}
}