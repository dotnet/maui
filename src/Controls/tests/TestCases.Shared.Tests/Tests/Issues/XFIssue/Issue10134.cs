using NUnit.Framework;
using NUnit.Framework.Legacy;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.TestCases.Tests.Issues;

public class Github10134 : _IssuesUITest
{
	public Github10134(TestDevice testDevice) : base(testDevice)
	{
	}

	public override string Issue => "Shell Top Tabbar focus issue";

	// [Test]
	// [Category(UITestCategories.Shell)]
	// [FailsOnIOS]
	// public void TopTabsDontScrollBackToStartWhenSelected() 
	// {
	// 	var element1 = App.WaitForElement("Tab 1", "Shell hasn't loaded")[0].Rect;
	// 	App.WaitForNoElement("Tab 12", "Tab shouldn't be visible");

	// 	Xamarin.UITest.Queries.AppRect element2 = element1;

	// 	for (int i = 2; i < 20; i++)
	// 	{
	// 		var results = App.Query($"Tab {i}");

	// 		if (results.Length == 0)
	// 			break;

	// 		element2 = results[0].Rect;
	// 	}

	// 	App.DragCoordinates(element2.CenterX, element2.CenterY, element1.CenterX, element1.CenterY);

	// 	App.WaitForNoElement("Tab 1");
	// 	bool testPassed = false;

	// 	// figure out what tabs are visible
	// 	for (int i = 20; i > 1; i--)
	// 	{
	// 		var results = App.Query($"Tab {i}");

	// 		if (results.Length > 0)
	// 		{
	// 			App.Tap($"Tab {i}");
	// 			App.WaitForElement($"Tab {i}");
	// 			testPassed = true;
	// 			break;
	// 		}
	// 	}

	// 	App.WaitForNoElement("Tab 1");
	// 	Assert.IsTrue(testPassed);
	// }
}