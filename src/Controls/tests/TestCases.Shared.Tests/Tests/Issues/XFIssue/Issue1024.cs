using NUnit.Framework;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.TestCases.Tests.Issues;

public class Issue1024 : _IssuesUITest
{
	public Issue1024(TestDevice testDevice) : base(testDevice)
	{
	}

	public override string Issue => "Entry and Editor are leaking when used in ViewCell";

	[Test]
	[Category(UITestCategories.Performance)]
	public void Bugzilla1024Test()
	{
		for (var n = 0; n < 10; n++)
		{
			App.WaitForElement("Push");
			App.Tap("Push");

			App.WaitForElement("ListView");
			App.TapBackArrow();

		}

		// At this point, the counter can be any value, but it's most likely not zero.
		// Invoking GC once is enough to clean up all garbage data and set counter to zero
		App.WaitForElement("GC");
		App.Tap("GC");

		Assert.That(App.FindElement("Counter").GetText(), Is.EqualTo("Counter: 0"));
	}
}