// On Windows, AutomationId for Layout elements is not working (https://github.com/dotnet/maui/issues/4715)
#if TEST_FAILS_ON_WINDOWS
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

			// Wait until Item1 width has actually changed to ensure the layout update completed
			var timeout = System.TimeSpan.FromSeconds(5);
			var pollDelay = System.TimeSpan.FromMilliseconds(100);
			var stopwatch = System.Diagnostics.Stopwatch.StartNew();

			while (stopwatch.Elapsed < timeout)
			{
				var currentRect = App.FindElement("Item1").GetRect();
				if (System.Math.Abs(currentRect.Width - item1Before.Width) > 1)
				{
					break;
				}

				System.Threading.Thread.Sleep(pollDelay);
			}

			// Verify the width actually changed; fail here rather than in downstream assertions
			var item1Check = App.FindElement("Item1").GetRect();
			Assert.That(item1Check.Width, Is.Not.EqualTo(item1Before.Width).Within(1),
				"Item1 width should have changed within timeout after tapping ChangeWidths");

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
#endif
