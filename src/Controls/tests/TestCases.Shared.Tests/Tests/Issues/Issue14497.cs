using NUnit.Framework;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.TestCases.Tests.Issues;

public class Issue14497 : _IssuesUITest
{
	public Issue14497(TestDevice device) : base(device) { }

	public override string Issue => "Dynamically setting SearchHandler Query property does not update text in the search box";
	const string ChangeSearchText = "ChangeSearchText";

	[Test]
	[Category(UITestCategories.Shell)]
	public void DynamicallyQueryNotUpdating()
	{
		App.WaitForElement(ChangeSearchText);
		App.Tap(ChangeSearchText);
		var searchHandlerString = App.GetShellSearchHandler().GetText();
		Assert.That(searchHandlerString, Is.EqualTo("Hello World"));
	}
}