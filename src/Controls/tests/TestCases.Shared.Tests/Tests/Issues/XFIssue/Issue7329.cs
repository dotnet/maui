#if TEST_FAILS_ON_CATALYST  //In Catalyst, `ScrollDown` isn't functioning correctly with Appium.
using NUnit.Framework;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.TestCases.Tests.Issues;

public class Issue7329 : _IssuesUITest
{
	public Issue7329(TestDevice testDevice) : base(testDevice)
	{
	}

	public override string Issue => "[Android] ListView scroll not working when inside a ScrollView";

	[Test]
	[Category(UITestCategories.ScrollView)]
	public void ScrollListViewInsideScrollView()
	{
		App.WaitForElement("1");

		App.ScrollDown("NestedListView");

#if IOS
		// In iOS, WaitForNoElement throws a timeout exception eventhough the text is not visible on the screen. So using the ListView Items to ensure scroll.		
		App.WaitForElement("80");
#else
		// App.QueryUntilPresent isn't functioning correctly; it throws a timeout exception immediately after the first try.
		// Verifying that instructions label is not visible also confirms that the ListView has scrolled.
		App.WaitForNoElement("If the List View can scroll the test has passed");
#endif
	}
}
#endif