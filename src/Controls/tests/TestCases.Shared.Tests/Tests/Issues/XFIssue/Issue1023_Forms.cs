using NUnit.Framework;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.TestCases.Tests.Issues;

public class Issue1023_Forms : _IssuesUITest
{
	public Issue1023_Forms(TestDevice testDevice) : base(testDevice)
	{
	}

	public override string Issue => "Automate GC checks of picker disposals";

	[Test]
	[Category(UITestCategories.Picker)]
	public void Bugzilla1023Test()
	{
		for (var n = 0; n < 10; n++)
		{
			App.WaitForElement("Push");
			App.Tap("Push");

			App.WaitForElement("ListView");
#if IOS //Getting null reference exception while tap the back button without WaitForElement in iOS.
			App.WaitForElement("Back");
#endif
			App.TapBackArrow();

		}

		// At this point, the counter can be any value, but it's most likely not zero.
		// Invoking GC once is enough to clean up all garbage data and set counter to zero
		App.WaitForElement("GC");
		App.Tap("GC");

		Assert.That(App.WaitForElement("Counter").GetText(), Is.EqualTo("Counter: 0"));
	}
}