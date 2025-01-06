using NUnit.Framework;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.TestCases.Tests.Issues;

public class Issue3019 : _IssuesUITest
{
	public Issue3019(TestDevice testDevice) : base(testDevice)
	{
	}

	public override string Issue => "Grouped ListView Header empty for adding items";

	[Test]
	[Category(UITestCategories.ListView)]
	public void MakeSureListGroupShowsUpAndItemsAreClickable()
	{
		App.WaitForElement("Grouped Item: 0");
		App.Tap("Grouped Item: 0");
		Assert.That(App.WaitForElement("MessageLabel").GetText(), Is.EqualTo("Grouped Item: 0 Clicked"));
		App.Tap("Grouped Item: 1");
		Assert.That(App.WaitForElement("MessageLabel").GetText(), Is.EqualTo("Grouped Item: 1 Clicked"));
	}
}
