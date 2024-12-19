#if ANDROID || IOS
using NUnit.Framework;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.TestCases.Tests.Issues
{
	public class Issue25256 : _IssuesUITest
	{
		public Issue25256(TestDevice testDevice) : base(testDevice)
		{
		}

		public override string Issue => "ListView ScrolledEventArgs.ScrollY does not reset when ItemsSource changes";

		[Test]
		[Category(UITestCategories.ListView)]
		public void ListViewShouldHaveProperScrollYAfterResetingItemsSource()
		{
			App.WaitForElement("button");
			App.ScrollDown("list", ScrollStrategy.Gesture, swipePercentage: 0.5, swipeSpeed: 100);
			App.ScrollDown("groupedList", ScrollStrategy.Gesture, swipePercentage: 0.5, swipeSpeed: 100);
			App.Click("button");

			var label = App.FindElement("scrollYLabel").GetText();
			var groupedListlabel = App.FindElement("scrollYLabelGroupedList").GetText();
			Assert.That(label, Is.EqualTo("0"));
			Assert.That(groupedListlabel, Is.EqualTo("0"));
		}
	}
}
#endif