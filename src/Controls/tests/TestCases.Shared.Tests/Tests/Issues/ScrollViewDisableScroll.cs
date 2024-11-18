#if !MACCATALYST && !IOS && !ANDROID && !WINDOWS
using NUnit.Framework;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.TestCases.Tests.Issues
{
	[Category(UITestCategories.ScrollView)]
	public class ScrollViewDisableScrollUITests : _IssuesUITest
	{
		const string ScrollViewMark = "ScrollView";
		const string FirstItemMark = "FirstItem";
		const string ToggleButtonMark = "ToggleButton";

		public ScrollViewDisableScrollUITests(TestDevice device)
			: base(device)
		{
		}

		public override string Issue => "Add VerticalScrollMode/HorizontalScrollMode to ListView and ScrollView";

		// Issue2680Test_ScrollDisabled (src\Compatibility\ControlGallery\src\Issues.Shared\Issue2680ScrollView.cs)
		[Test]
		[Category(UITestCategories.ScrollView)]
		[FailsOnAndroidWhenRunningOnXamarinUITest("Currently fails on Android; see https://github.com/dotnet/maui/issues/19213")]
		[FailsOnIOSWhenRunningOnXamarinUITest("Currently fails https://github.com/dotnet/maui/pull/19181")]
		[FailsOnMacWhenRunningOnXamarinUITest("Currently fails https://github.com/dotnet/maui/pull/19181")]
		[FailsOnWindowsWhenRunningOnXamarinUITest("Currently fails https://github.com/dotnet/maui/pull/19181")]
		public void ScrollDisabled()
		{
			App.WaitForElement(FirstItemMark);

			// Waiting until merge https://github.com/dotnet/maui/pull/19181
			//App.ScrollDown(ScrollViewMark);
			//App.ScrollDown(ScrollViewMark);

			App.WaitForElement(FirstItemMark, timeout: TimeSpan.FromSeconds(5));
		}

		// Issue2680Test_ScrollEnabled (src\Compatibility\ControlGallery\src\Issues.Shared\Issue2680ScrollView.cs)
		[Test]
		[Category(UITestCategories.ScrollView)]
		[FailsOnAllPlatformsWhenRunningOnXamarinUITest("Currently fails https://github.com/dotnet/maui/pull/19181")]
		public void ScrollEnabled()
		{			
			App.WaitForElement(FirstItemMark);

			App.Tap(ToggleButtonMark);

			// Waiting until merge https://github.com/dotnet/maui/pull/19181
			//App.ScrollDown(ScrollViewMark);
			//App.ScrollDown(ScrollViewMark);

			App.WaitForNoElement(FirstItemMark, timeout: TimeSpan.FromSeconds(5));
		}
	}
}
#endif