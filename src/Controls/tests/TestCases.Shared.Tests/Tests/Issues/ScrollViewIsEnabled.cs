using NUnit.Framework;
using NUnit.Framework.Legacy;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.TestCases.Tests.Issues
{
	[Category(UITestCategories.ScrollView)]
	public class ScrollViewIsEnabledTests : _IssuesUITest
	{
		const string InitiallyEnabled = "InitiallyEnabled";
		const string InitiallyNotEnabled = "InitiallyNotEnabled";
		const string ToggleButton = "ToggleButton";
		const string ScrollView = "TheScrollView";
		const string FirstItem = "FirstItem";
		const string Success = "Success";

		public ScrollViewIsEnabledTests(TestDevice device)
			: base(device)
		{
		}

		public override string Issue => "ScrollView set to disabled will still allow scrolling";

		// ScrollViewInitiallyEnabled (src\Compatibility\ControlGallery\src\Issues.Shared\ScrollViewIsEnabled.cs)
		[Test]
		public void ScrollViewInitiallyEnabled()
		{
			// 1. Enable the ScrollView.
			App.WaitForElement(InitiallyEnabled);
			App.Tap(InitiallyEnabled);
			App.WaitForElement(FirstItem);
			App.WaitForElement(ScrollView);

			// 2. Scroll a litlle bit.
			App.ScrollDown("Item10", ScrollStrategy.Gesture);

			// 3. If the ScrollView scrolled, the test passed.
			var success = App.WaitForElement(Success).GetText();
			Assert.That("Success", Is.EqualTo(success)); // If the ScrollView scrolled, the success label should be displayed

			App.TapBackArrow();

		}

		// ScrollViewInitiallyEnabledThenDisabled (src\Compatibility\ControlGallery\src\Issues.Shared\ScrollViewIsEnabled.cs)
		[Test]
		public void ScrollViewInitiallyEnabledThenDisabled()
		{
			// 1. Enable the ScrollView.
			App.WaitForElement(InitiallyEnabled);
			App.Tap(InitiallyEnabled);

			// 2. Disable the ScrollView.
			App.WaitForElement(ToggleButton);
			App.Tap(ToggleButton);

			// Scrolling should now be IsEnabled = false

			// 3. Try to scroll.
			App.WaitForElement(FirstItem);
			App.WaitForElement(ScrollView);
			App.ScrollDown("Item10", ScrollStrategy.Gesture);


			var success = App.WaitForElement(Success).GetText();
			Assert.That("Initial Text", Is.EqualTo(success));

			App.TapBackArrow();
		}

		// ScrollViewInitiallyNotEnabled (src\Compatibility\ControlGallery\src\Issues.Shared\ScrollViewIsEnabled.cs)
		[Test]
		public void ScrollViewInitiallyNotEnabled()
		{
			// 1. Disable the ScrollView.
			App.WaitForElement(InitiallyNotEnabled);
			App.Tap(InitiallyNotEnabled);
			App.WaitForElement(FirstItem);
			App.WaitForElement(ScrollView);
			App.ScrollDown("Item10", ScrollStrategy.Gesture);

			// 2. Shouldn't have scrolled.
			// Shouldn't have scrolled, so no success label should be displayed
			var success = App.WaitForElement(Success).GetText();
			Assert.That("Initial Text", Is.EqualTo(success));

			App.TapBackArrow();
		}

		// ScrollViewInitiallyNotEnabledThenEnabled (src\Compatibility\ControlGallery\src\Issues.Shared\ScrollViewIsEnabled.cs)
		[Test]
		public void ScrollViewInitiallyNotEnabledThenEnabled()
		{
			// 1. Disable the ScrollView.
			App.WaitForElement(InitiallyNotEnabled);
			App.Tap(InitiallyNotEnabled);

			// 2. Enable the ScrollView.
			App.WaitForElement(ToggleButton);
			App.Tap(ToggleButton);

			// Scrolling should now be IsEnabled = true

			App.WaitForElement(FirstItem);
			App.WaitForElement(ScrollView);

			// 3. Try to scroll.
			App.ScrollDown("Item10", ScrollStrategy.Gesture);

			// 4. If the ScrollView scrolled, the test passed.
			var success = App.WaitForElement(Success).GetText();
			Assert.That("Success", Is.EqualTo(success)); // If the ScrollView scrolled, the success label should be displayed

			App.TapBackArrow();
		}
	}
}