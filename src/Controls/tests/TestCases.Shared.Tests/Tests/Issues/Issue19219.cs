using NUnit.Framework;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.TestCases.Tests.Issues;

public class Issue19219 : _IssuesUITest
{
	public override string Issue => "[Android, iOS, macOS] Shell SearchHandler Command Not Executed on Item Selection";

	public Issue19219(TestDevice device) : base(device)
	{
	}

	[Test]
	[Category(UITestCategories.Shell)]
	[Category(UITestCategories.SearchBar)]
	public void ShouldExecuteCommandWhenTappingShellSearchItem()
	{
		App.WaitForElement("SetQueryButton");
		var searchHandler = App.GetShellSearchHandler().GetRect();
		App.Tap("SetQueryButton");
		App.TapCoordinates(searchHandler.X + 100, searchHandler.Y + 10);
#if ANDROID
		App.TapCoordinates(searchHandler.X + 100, searchHandler.Height + 100);
#else
		App.WaitForElement("Los Angeles");
		App.Tap("Los Angeles");
#endif
		VerifyScreenshot();

	}
}