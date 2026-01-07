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
		App.WaitForElement("SearchHandlerLabel");
		var searchHandler = App.GetShellSearchHandler();
		searchHandler.Tap();
		searchHandler.SendKeys("Los Angeles");
#if ANDROID // Android does not support selecting elements in SearchHandler's results so used tap coordinates
		var y = searchHandler.GetRect().Y + searchHandler.GetRect().Height;
		App.TapCoordinates(searchHandler.GetRect().X + 10, y + 10);
#else
		App.Tap("Los Angeles");
#endif
		var text = App.WaitForElement("SearchHandlerLabel").GetText();
		Assert.That(text, Is.EqualTo("SearchHandler Command Executed when tap on item"));
	}
}