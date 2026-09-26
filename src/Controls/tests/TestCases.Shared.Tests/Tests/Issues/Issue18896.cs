using NUnit.Framework;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.TestCases.Tests.Issues;

public class Issue18896 : _IssuesUITest
{
	const string ListView = "TestListView";

	public Issue18896(TestDevice device) : base(device)
	{
	}

	public override string Issue => "Can scroll ListView inside RefreshView";

	[Test]
	[Category(UITestCategories.RefreshView)]
	public void Issue18896Test()
	{
		App.WaitForElement("WaitForStubControl");

#if WINDOWS
		App.WaitForElement(ListView);
		Assert.That(() => GetVerticalScrollPercent(), Is.Zero.After(5000, 200));
#endif
		App.ScrollDown(ListView);
#if ANDROID
		App.ScrollUp(ListView, ScrollStrategy.Gesture, 0.9);
#elif WINDOWS
		Assert.That(() => GetVerticalScrollPercent(), Is.GreaterThan(0).After(5000, 200),
			"The ListView inside RefreshView must actually scroll.");

		// Opposite touch gestures can leave a residual offset instead of returning to the top.
		for (int scrollCount = 0; scrollCount < 3; scrollCount++)
		{
			App.ScrollUp(ListView);
			if (GetVerticalScrollPercent() == 0)
				break;
		}

		Assert.That(() => GetVerticalScrollPercent(), Is.Zero.After(5000, 200),
			"The ListView must return to the top before comparing its rendered rows.");
#else
		App.ScrollUp(ListView);
#endif
		// ListView with HasUnevenRows may have variable height row rendering that requires
		// additional time for images to load and scrollbar to disappear.
		// Use retryTimeout to adaptively wait for the UI to stabilize.
		VerifyScreenshot(retryTimeout: TimeSpan.FromSeconds(5));
	}

#if WINDOWS
	double GetVerticalScrollPercent()
	{
		var document = System.Xml.Linq.XDocument.Parse(App.ElementTree);
		var list = document.Descendants("List")
			.Single(element => (string?)element.Attribute("AutomationId") == ListView);
		return (double)(list.Attribute("VerticalScrollPercent")
			?? throw new InvalidOperationException("The native ListView did not report its vertical scroll position."));
	}
#endif
}