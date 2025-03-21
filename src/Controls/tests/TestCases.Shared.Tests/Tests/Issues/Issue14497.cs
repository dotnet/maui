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
#if MACCATALYST || IOS
		Assert.That(App.WaitForElement(AppiumQuery.ByXPath("//XCUIElementTypeSearchField")).GetText(), Is.EqualTo("Hello World"));
#elif ANDROID
		Assert.That(App.WaitForElement(AppiumQuery.ByXPath("//android.widget.EditText")).GetText(), Is.EqualTo("Hello World"));
#else
		Assert.That(App.WaitForElement("TextBox").GetText(), Is.EqualTo("Hello World"));
#endif
	}
}