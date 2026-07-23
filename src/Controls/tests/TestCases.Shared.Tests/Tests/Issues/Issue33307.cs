using NUnit.Framework;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.TestCases.Tests.Issues;
public class Issue33307 : _IssuesUITest
{
	public Issue33307(TestDevice device) : base(device) { }

	public override string Issue => "The Picker is still binding to the property and reacts to data changes after the page is closed.";
	[Test]
	[Category(UITestCategories.Picker)]
	public void VerifyPickerItemsinNavigation()
	{
		App.WaitForElement("Page1");
		App.Tap("Page1");
		App.WaitForElement("AddItems");
		App.Tap("AddItems");
		App.TapBackArrow();
		App.WaitForElement("Page2");
		App.Tap("Page2");
		App.WaitForElement("Add");
		App.Tap("Add");
		App.WaitForElement("SelectSecondItem");
		App.Tap("SelectSecondItem");
		App.TapBackArrow();
		App.WaitForElement("Page1");
		App.Tap("Page1");
		App.WaitForElement("DeleteItem");
		App.Tap("DeleteItem");
		App.TapBackArrow();
		App.WaitForElement("Page2");
		App.Tap("Page2");
		App.WaitForElement("StatusLabel");
		Assert.That(App.FindElement("StatusLabel").GetText(), Is.EqualTo("None"));
	}
}
