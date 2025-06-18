using NUnit.Framework;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.TestCases.Tests.Issues;

public class Bugzilla44886 : _IssuesUITest
{
	const string CountId = "countId";

	public Bugzilla44886(TestDevice testDevice) : base(testDevice)
	{
	}

	public override string Issue => "UWP Listview ItemSelected event triggered twice for each selection";

	[Test]
	[Category(UITestCategories.ListView)]
	public void Bugzilla44886Test()
	{
		App.WaitForElement("Item 1");
		App.Tap("Item 1");

		int count = int.Parse(App.FindElement(CountId)?.GetText() ?? "0");

		Assert.That(count, Is.EqualTo(1));
	}
}