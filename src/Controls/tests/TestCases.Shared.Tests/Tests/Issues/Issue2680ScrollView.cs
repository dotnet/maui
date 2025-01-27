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
		public void Issue2680Test_ScrollDisabled()
		{
			var label = App.WaitForElement(FirstItemMark);
			App.ScrollDown(ScrollViewMark);
			Assert.That(label.GetText(), Is.EqualTo("Not scrolled"));
		}

		[Test]
		[Category(UITestCategories.ScrollView)]
		public void Issue2680Test_ScrollEnabled()
		{
			var label = App.WaitForElement(FirstItemMark);
			App.Tap(ToggleButtonMark);
			App.ScrollDown(ScrollViewMark);
			Assert.That(label.GetText(), Is.EqualTo("Scrolled"));
		}
	}
}
