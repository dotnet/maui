#if TEST_FAILS_ON_CATALYST && TEST_FAILS_ON_WINDOWS && TEST_FAILS_ON_ANDROID
// DragCoordinates is not supported on MacCatalyst
// On Windows this test case is not valid due to the tob tabs are visbile in drop-down.
// On Android FindElements are not working as expected in Appium. It always returns 0 elements.
using NUnit.Framework;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.TestCases.Tests.Issues;

public class Github10134 : _IssuesUITest
{
	public Github10134(TestDevice testDevice) : base(testDevice)
	{
	}

	public override string Issue => "Shell Top Tabbar focus issue";

	[Test]
	[Category(UITestCategories.Shell)]
	public void TopTabsDontScrollBackToStartWhenSelected()
	{
		App.WaitForElement("Tab 1");
		var element1 = App.WaitForElement("Tab 1").GetRect();
		App.WaitForNoElement("Tab 12");

		var element2 = element1;

		for (int i = 2; i < 20; i++)
		{
			var results = App.FindElements($"Tab {i}");

			foreach (var result in results)
			{
				element2 = result.GetRect();
				break;
			}

			if (results.Count == 0)
				break;
		}
		
		App.DragCoordinates(element2.CenterX(), element2.CenterY(), element1.CenterX(), element1.CenterY());

		App.WaitForNoElement("Tab 1");
		bool testPassed = false;
		for (int i = 20; i > 1; i--)
		{
			var results = App.FindElements($"Tab {i}");

			if (results.Count > 0)
			{
				App.Tap($"Tab {i}");
				App.WaitForElement($"Tab {i}");
				testPassed = true;
				break;
			}
		}
		App.WaitForNoElement("Tab 1");
		Assert.That(testPassed, Is.True);

	}
}
#endif