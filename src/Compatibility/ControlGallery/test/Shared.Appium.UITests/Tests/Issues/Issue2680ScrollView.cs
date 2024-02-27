using NUnit.Framework;
using UITest.Appium;

namespace UITests.Tests.Issues
{
	public class Issue2680ScrollView : IssuesUITest
	{
		const string ScrollViewMark = "ScrollView";
		const string FirstItemMark = "FirstItem";
		const string ToggleButtonMark = "ToggleButton";

		public Issue2680ScrollView(TestDevice testDevice) : base(testDevice)
		{
		}

		public override string Issue => "[Enhancement] Add VerticalScrollMode/HorizontalScrollMode to ListView and ScrollView";

		[Test]
		public void Issue2680Test_ScrollDisabled()
		{
			App.ScrollDown(ScrollViewMark);
			App.ScrollDown(ScrollViewMark);

			App.WaitForElement(FirstItemMark, timeout: TimeSpan.FromSeconds(5));
		}

		[Test]
		public void Issue2680Test_ScrollEnabled()
		{
			App.Click(ToggleButtonMark);

			App.ScrollDown(ScrollViewMark);
			App.ScrollDown(ScrollViewMark);

			App.WaitForNoElement(FirstItemMark, timeout: TimeSpan.FromSeconds(5));
		}
	}
}