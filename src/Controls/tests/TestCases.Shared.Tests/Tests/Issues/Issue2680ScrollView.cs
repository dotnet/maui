#if TEST_FAILS_ON_WINDOWS && TEST_FAILS_ON_IOS && TEST_FAILS_ON_CATALYST // In iOS and Catalyst, WaitForNoElement throws a timeout exception eventhough the text is not visible on the screen by scrolling.
//In Windows, The ScrollView remains scrollable even when ScrollOrientation.Neither is set. Issue Link: https://github.com/dotnet/maui/issues/27140
using NUnit.Framework;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.TestCases.Tests.Issues
{
	public class Issue2680ScrollView : _IssuesUITest
	{
		const string ScrollViewMark = "ScrollView";
		const string FirstItemMark = "FirstItem";
		const string ToggleButtonMark = "ToggleButton";

		public Issue2680ScrollView(TestDevice testDevice) : base(testDevice)
		{
		}

		public override string Issue => "[Enhancement] Add VerticalScrollMode/HorizontalScrollMode to ListView and ScrollView";

		[Test]
		[Category(UITestCategories.ScrollView)]
		[FailsOnIOSWhenRunningOnXamarinUITest]
		[FailsOnMacWhenRunningOnXamarinUITest]
		public void Issue2680Test_ScrollDisabled()
		{
			App.WaitForElement(ScrollViewMark);
			App.ScrollDown(ScrollViewMark);
			App.ScrollDown(ScrollViewMark);

			App.WaitForElement(FirstItemMark, timeout: TimeSpan.FromSeconds(5));
		}

		[Test]
		[Category(UITestCategories.ScrollView)]
		[FailsOnIOSWhenRunningOnXamarinUITest]
		[FailsOnMacWhenRunningOnXamarinUITest]
		[FailsOnWindowsWhenRunningOnXamarinUITest]
		public void Issue2680Test_ScrollEnabled()
		{
			App.WaitForElement(ToggleButtonMark);
			App.Tap(ToggleButtonMark);

			App.ScrollDown(ScrollViewMark);
			App.ScrollDown(ScrollViewMark);

			App.WaitForNoElement(FirstItemMark, timeout: TimeSpan.FromSeconds(5));
		}
	}
}
#endif