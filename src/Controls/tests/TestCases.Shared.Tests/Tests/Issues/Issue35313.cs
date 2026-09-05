#if ANDROID // Open Issue For iOS, MacCatalyst and Windows : https://github.com/dotnet/maui/issues/35326
using NUnit.Framework;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.TestCases.Tests.Issues;

public class Issue35313 : _IssuesUITest
{
	public Issue35313(TestDevice device) : base(device) { }

	public override string Issue => "ScrollTo(0) not working on grouped CollectionView";

	[Test]
	[Category(UITestCategories.CollectionView)]
	public void GroupedCollectionViewScrollToIndexZeroShouldScrollToStart()
	{
		App.WaitForElement("ScrollToEndButton");

		// Scroll to the end so the top is off screen
		App.Tap("ScrollToEndButton");
		App.WaitForElement("Group 5 — Item 10");

		// ScrollTo(0) on a grouped CollectionView — this is the regression
		// Without the fix it silently does nothing on Android
		App.Tap("ScrollToStartButton");

		App.WaitForElement("Group 1 — Item 1");
	}
}
#endif