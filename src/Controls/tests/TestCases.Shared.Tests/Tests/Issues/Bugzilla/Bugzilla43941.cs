using NUnit.Framework;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.TestCases.Tests.Issues;

public class Bugzilla43941 : _IssuesUITest
{
	public Bugzilla43941(TestDevice testDevice) : base(testDevice)
	{
	}

	public override string Issue => "Memory leak with ListView's RecycleElement on iOS";

	[Test]
	[Category(UITestCategories.ListView)]
	public void Bugzilla43941Test()
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

		var i = 0;
		while (!App.FindElement("counterlabel")?.GetText()?.Equals("Counter: 0",
			StringComparison.OrdinalIgnoreCase) ?? false && i < 10)
		{
			i++;

			App.Tap("GC");
		}
	}
}