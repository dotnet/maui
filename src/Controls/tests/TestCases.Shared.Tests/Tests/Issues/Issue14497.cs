using NUnit.Framework;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.TestCases.Tests.Issues;

public class Issue14497 : _IssuesUITest
{
	public Issue14497(TestDevice device) : base(device) { }

	public override string Issue => "Dynamically setting SearchHandler Query property does not update text in the search box";

	[Test]
	[Category(UITestCategories.Shell)]
	[Category(UITestCategories.SearchBar)]
	public void DynamicallyQueryNotUpdating()
	{

		App.WaitForElement("Button_ChangeSearchText");
		App.Tap("Button_ChangeSearchText");
		VerifyScreenshot();
	}
}