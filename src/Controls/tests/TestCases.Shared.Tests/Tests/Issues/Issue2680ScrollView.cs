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
		[Category(UITestCategories.Compatibility)]
		[FailsOnAndroid]
		[FailsOnIOS]
		[FailsOnMac]
		public void Issue2680Test_ScrollDisabled()
		{
			App.WaitForElement(ScrollViewMark);
			App.ScrollDown(ScrollViewMark);
			App.ScrollDown(ScrollViewMark);

			App.WaitForElement(FirstItemMark, timeout: TimeSpan.FromSeconds(5));
		}

		[Test]
		[Category(UITestCategories.ScrollView)]
		[FailsOnIOS]
		[FailsOnMac]
		[FailsOnWindows]
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