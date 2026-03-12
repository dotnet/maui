using NUnit.Framework;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.TestCases.Tests.Issues
{
	[Category(UITestCategories.Layout)]
	public class Issue31109 : _IssuesUITest
	{
		public Issue31109(TestDevice device) : base(device)
		{
		}

		public override string Issue => "FlexLayout items with Dynamic Width are not updating correctly on orientation change or scroll in Android";

		[Test]
		public void DynamicWidthRequestUpdatesFlexLayoutItems()
		{
			// Wait for the initial layout to render
			App.WaitForElement("Item1");
			App.WaitForElement("Item2");
			App.WaitForElement("Item3");

			// Get initial widths
			var item1Before = App.WaitForElement("Item1").GetRect();
			var item2Before = App.WaitForElement("Item2").GetRect();

			// All items should have different initial widths
			Assert.That(item1Before.Width, Is.LessThan(item2Before.Width),
				"Item1 should be narrower than Item2 initially");

			// Change all widths to the same value
			App.WaitForElement("ChangeWidthsButton");
			App.Tap("ChangeWidthsButton");

			// Wait for status label to update confirming the click happened
			App.WaitForElement("StatusLabel");

			// After changing widths, all items should have equal width
			var item1After = App.WaitForElement("Item1").GetRect();
			var item2After = App.WaitForElement("Item2").GetRect();
			var item3After = App.WaitForElement("Item3").GetRect();

			// All items should now have the same width (within tolerance for density conversion)
			Assert.That(item1After.Width, Is.EqualTo(item2After.Width).Within(2),
				"After width change, Item1 and Item2 should have equal widths");
			Assert.That(item2After.Width, Is.EqualTo(item3After.Width).Within(2),
				"After width change, Item2 and Item3 should have equal widths");

			// Widths should be larger than the initial smallest width
			Assert.That(item1After.Width, Is.GreaterThan(item1Before.Width),
				"Item1 should be wider after changing WidthRequest");
		}
	}
}
