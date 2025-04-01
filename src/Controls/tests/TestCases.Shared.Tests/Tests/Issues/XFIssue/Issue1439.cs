using NUnit.Framework;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.TestCases.Tests.Issues;

public class Issue1439 : _IssuesUITest
{
	public Issue1439(TestDevice testDevice) : base(testDevice)
	{
	}

	public override string Issue => "ItemTapped event for a grouped ListView is not working as expected.";

	[Test]
	[Category(UITestCategories.TableView)]
	public void Issue1439Test()
	{
		App.WaitForElement("A");
		App.Tap("A");

		Assert.That(App.FindElement("lblItem").GetText(), Is.EqualTo("A"));
		Assert.That(App.FindElement("lblGroup").GetText(), Is.EqualTo("Group 1"));

		App.Tap("B");

		Assert.That(App.FindElement("lblItem").GetText(), Is.EqualTo("B"));
		Assert.That(App.FindElement("lblGroup").GetText(), Is.EqualTo("Group 1"));

		App.Tap("C");

		Assert.That(App.FindElement("lblItem").GetText(), Is.EqualTo("C"));
		Assert.That(App.FindElement("lblGroup").GetText(), Is.EqualTo("Group 2"));

		App.Tap("D");

		Assert.That(App.FindElement("lblItem").GetText(), Is.EqualTo("D"));
		Assert.That(App.FindElement("lblGroup").GetText(), Is.EqualTo("Group 2"));
	}
}