#if TEST_FAILS_ON_WINDOWS // Related issue for Windows: https://github.com/dotnet/maui/issues/24482
using NUnit.Framework;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.TestCases.Tests.Issues
{
	public class CarouselViewProgrammaticPositionBounceBack : _IssuesUITest
	{
		const string PositionLabelId = "positionLabel";
		const string PositionEventCountId = "positionEventCount";
		const string CurrentItemEventCountId = "currentItemEventCount";
		const string SetPositionBtnId = "setPositionBtn";
		const string ReloadBtnId = "reloadBtn";

		public CarouselViewProgrammaticPositionBounceBack(TestDevice device)
			: base(device) { }

		public override string Issue =>
			"Programmatic Position/CurrentItem set bounces back; ItemsSource reset to 0";

		// Repros https://github.com/dotnet/maui/issues/21480
		[Test]
		[Category(UITestCategories.CarouselView)]
		public void SettingPositionProgrammaticallyDoesNotBounceBack()
		{
			App.WaitForElement("Item 0", timeout: TimeSpan.FromSeconds(30));
			App.WaitForElement(SetPositionBtnId);

			App.Tap(SetPositionBtnId);

			// Wait for the scroll/animation to settle and the position label to reflect the new value.
			App.WaitForElement("Item 3", timeout: TimeSpan.FromSeconds(10));
			App.WaitForElement("Pos:3", timeout: TimeSpan.FromSeconds(5));

			var posText = App.FindElement(PositionLabelId).GetText();
			Assert.That(
				posText,
				Is.EqualTo("Pos:3"),
				"Position should be 3 after programmatic assignment, not bounce back to another value."
			);

			// PositionChanged should fire at most once for the user-driven assignment,
			// not the intermediate "storm" observed in #21480. Allow a small tolerance
			// because some platforms may fire a 0→0 set-from-renderer on initial layout.
			var eventText = App.FindElement(PositionEventCountId).GetText();
			var eventCount = int.Parse(eventText.Replace("PosChanged:", ""));
			Assert.That(
				eventCount,
				Is.LessThanOrEqualTo(2),
				$"PositionChanged fired {eventCount} times — expected ≤ 2 (1 real + possible initial echo)."
			);

			// CurrentItemChanged should also fire at most once for the same user-driven assignment.
			var currentItemText = App.FindElement(CurrentItemEventCountId).GetText();
			var currentItemCount = int.Parse(currentItemText.Replace("ItemChanged:", ""));
			Assert.That(
				currentItemCount,
				Is.LessThanOrEqualTo(2),
				$"CurrentItemChanged fired {currentItemCount} times — expected ≤ 2."
			);
		}

		// Repros the "Critical Issue" in https://github.com/dotnet/maui/issues/23023
		[Test]
		[Category(UITestCategories.CarouselView)]
		public void ItemsSourceReloadHonorsExplicitPosition()
		{
			App.WaitForElement("Item 0", timeout: TimeSpan.FromSeconds(30));
			App.WaitForElement(ReloadBtnId);

			App.Tap(ReloadBtnId);

			// After ItemsSource reset + Position = 2 in the same operation, the carousel
			// must show Item 2, not snap back to Item 0.
			App.WaitForElement("Item 2", timeout: TimeSpan.FromSeconds(10));
			App.WaitForElement("Pos:2", timeout: TimeSpan.FromSeconds(5));

			var posText = App.FindElement(PositionLabelId).GetText();
			Assert.That(
				posText,
				Is.EqualTo("Pos:2"),
				"Explicit Position assignment in the same render as an ItemsSource reset must be honored."
			);
		}
	}
}
#endif
