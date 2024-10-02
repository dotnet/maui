﻿#if !IOS && !MACCATALYST
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

#if WINDOWS || ANDROID
		// ScrollViewInitiallyEnabled (src\Compatibility\ControlGallery\src\Issues.Shared\ScrollViewIsEnabled.cs)
		[Test]
		[FailsOnIOS("This test is failing, likely due to product issue")]
		[FailsOnMac("This test is failing, likely due to product issue")]
		public void ScrollViewInitiallyEnabled()
		{
			// 1. Enable the ScrollView.
			App.WaitForElement(InitiallyEnabled);
			App.Tap(InitiallyEnabled);
			App.WaitForElement(FirstItem);
			App.WaitForElement(ScrollView);

			// 2. Scroll a litlle bit.
			App.ScrollTo("Item10", true);

			// 3. If the ScrollView scrolled, the test passed.
			App.WaitForElement(Success); // If the ScrollView scrolled, the success label should be displayed

			this.Back();

		}

		// ScrollViewInitiallyEnabledThenDisabled (src\Compatibility\ControlGallery\src\Issues.Shared\ScrollViewIsEnabled.cs)
		[Test]
		[FailsOnIOS("This test is failing, likely due to product issue")]
		[FailsOnMac("This test is failing, likely due to product issue")]
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
			App.ScrollTo("Item10", true);

			// 4. Shouldn't have scrolled.
			var success = App.FindElement(Success).GetText(); // Shouldn't have scrolled, so no success label should be displayed
			ClassicAssert.IsEmpty(success);

			this.Back();
		}

		// ScrollViewInitiallyNotEnabled (src\Compatibility\ControlGallery\src\Issues.Shared\ScrollViewIsEnabled.cs)
		[Test]
		[FailsOnIOS("This test is failing, likely due to product issue")]
		[FailsOnMac("This test is failing, likely due to product issue")]
		public void ScrollViewInitiallyNotEnabled()
		{
			// 1. Disable the ScrollView.
			App.WaitForElement(InitiallyNotEnabled);
			App.Tap(InitiallyNotEnabled);
			App.WaitForElement(FirstItem);
			App.WaitForElement(ScrollView);
			App.ScrollTo("Item10", true);

			// 2. Shouldn't have scrolled.
			var success = App.FindElement(Success).GetText(); // Shouldn't have scrolled, so no success label should be displayed
			ClassicAssert.IsEmpty(success);

			this.Back();
		}

		// ScrollViewInitiallyNotEnabledThenEnabled (src\Compatibility\ControlGallery\src\Issues.Shared\ScrollViewIsEnabled.cs)
		[Test]
		[FailsOnIOS("This test is failing, likely due to product issue")]
		[FailsOnMac("This test is failing, likely due to product issue")]
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
			App.ScrollTo("Item10", true);

			// 4. If the ScrollView scrolled, the test passed.
			App.WaitForElement(Success); // If the ScrollView scrolled, the success label should be displayed

			this.Back();
		}
#endif
	}
}
#endif