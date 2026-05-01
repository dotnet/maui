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
		[Ignore(
			"Pending fix for https://github.com/dotnet/maui/issues/21480 (affects Android and iOS, per the linked issue). On origin/main this assertion observes PosChanged:4 instead of the expected PosChanged:1 because intermediate animated-scroll callbacks each fire CarouselView.PositionChanged, clobbering the user's Position assignment. Verified red on Android (API 35); not independently verified on iOS but expected to reproduce based on upstream issue reports."
		)]
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

			// PositionChanged should fire exactly once for the user-driven assignment, not multiple times.
			var eventText = App.FindElement(PositionEventCountId).GetText();
			Assert.That(
				eventText,
				Is.EqualTo("PosChanged:1"),
				"PositionChanged should fire exactly once when Position is set programmatically."
			);

			// CurrentItemChanged should also fire exactly once for the same user-driven assignment.
			var currentItemText = App.FindElement(CurrentItemEventCountId).GetText();
			Assert.That(
				currentItemText,
				Is.EqualTo("ItemChanged:1"),
				"CurrentItemChanged should fire exactly once when Position is set programmatically."
			);
		}

		// Repros the "Critical Issue" in https://github.com/dotnet/maui/issues/23023
		[Test]
		[Category(UITestCategories.CarouselView)]
		[Ignore(
			"Pending fix for https://github.com/dotnet/maui/issues/23023 (affects Android and iOS, per the linked issue). On origin/main, after assigning a new ItemsSource reference and Position=2 in the same operation, the carousel stays on Item 0 because UpdateAdapter synchronously forces Position=0 and CurrentItem=null, clobbering the user's explicit Position value. Verified red on Android (API 35); not independently verified on iOS but expected to reproduce based on upstream issue reports."
		)]
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
