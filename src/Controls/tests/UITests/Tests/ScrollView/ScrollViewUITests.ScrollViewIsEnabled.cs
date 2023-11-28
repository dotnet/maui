using NUnit.Framework;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.AppiumTests
{
	public class ScrollViewIsEnabledTests : ScrollViewUITests
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

		// ScrollViewInitiallyEnabled (src\Compatibility\ControlGallery\src\Issues.Shared\ScrollViewIsEnabled.cs)
		[Test]
		public void ScrollViewInitiallyEnabled()
		{
			App.Click("ScrollViewIsEnabled");

			// 1. Enable the ScrollView.
			App.WaitForElement(InitiallyEnabled);
			App.Click(InitiallyEnabled);
			App.WaitForElement(FirstItem);
			App.WaitForElement(ScrollView);

			// 2. Scroll a litlle bit.
			App.ScrollTo("Item10", true);

			// 3. If the ScrollView scrolled, the test passed.
			App.WaitForElement(Success); // If the ScrollView scrolled, the success label should be displayed
		}

		// ScrollViewInitiallyEnabledThenDisabled (src\Compatibility\ControlGallery\src\Issues.Shared\ScrollViewIsEnabled.cs)
		[Test]
		public void ScrollViewInitiallyEnabledThenDisabled()
		{
			App.Click("ScrollViewIsEnabled");

			// 1. Enable the ScrollView.
			App.WaitForElement(InitiallyEnabled);
			App.Click(InitiallyEnabled);

			// 2. Disable the ScrollView.
			App.WaitForElement(ToggleButton);
			App.Click(ToggleButton);

			// Scrolling should now be IsEnabled = false

			// 3. Try to scroll.
			App.WaitForElement(FirstItem);
			App.WaitForElement(ScrollView);
			App.ScrollTo("Item10", true);

			// 4. Shouldn't have scrolled.
			var success = App.FindElement(Success).GetText(); // Shouldn't have scrolled, so no success label should be displayed
			Assert.IsEmpty(success);
		}

		// ScrollViewInitiallyNotEnabled (src\Compatibility\ControlGallery\src\Issues.Shared\ScrollViewIsEnabled.cs)
		[Test]
		public void ScrollViewInitiallyNotEnabled()
		{
			App.Click("ScrollViewIsEnabled");

			// 1. Disable the ScrollView.
			App.WaitForElement(InitiallyNotEnabled);
			App.Click(InitiallyNotEnabled);
			App.WaitForElement(FirstItem);
			App.WaitForElement(ScrollView);
			App.ScrollTo("Item10", true);

			// 2. Shouldn't have scrolled.
			var success = App.FindElement(Success).GetText(); // Shouldn't have scrolled, so no success label should be displayed
			Assert.IsEmpty(success);
		}

		// ScrollViewInitiallyNotEnabledThenEnabled (src\Compatibility\ControlGallery\src\Issues.Shared\ScrollViewIsEnabled.cs)
		[Test]
		public void ScrollViewInitiallyNotEnabledThenEnabled()
		{
			App.Click("ScrollViewIsEnabled");

			// 1. Disable the ScrollView.
			App.WaitForElement(InitiallyNotEnabled);
			App.Click(InitiallyNotEnabled);

			// 2. Enaable the ScrollView.
			App.WaitForElement(ToggleButton);
			App.Click(ToggleButton);

			// Scrolling should now be IsEnabled = true

			App.WaitForElement(FirstItem);
			App.WaitForElement(ScrollView);

			// 3. Try to scroll.
			App.ScrollTo("Item10", true);

			// 4. If the ScrollView scrolled, the test passed.
			App.WaitForElement(Success); // If the ScrollView scrolled, the success label should be displayed
		}
	}
}