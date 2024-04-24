using NUnit.Framework;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.AppiumTests
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
		public void ScrollDisabled()
		{
			this.IgnoreIfPlatforms(new[]
			{
				TestDevice.Android,
			}, "Currently fails on Android; see https://github.com/dotnet/maui/issues/19213");

			this.IgnoreIfPlatforms(new[]
			{
				TestDevice.iOS, TestDevice.Mac, TestDevice.Windows
			}, "https://github.com/dotnet/maui/pull/19181");

			App.WaitForElement(FirstItemMark);

			// Waiting until merge https://github.com/dotnet/maui/pull/19181
			//App.ScrollDown(ScrollViewMark);
			//App.ScrollDown(ScrollViewMark);

			App.WaitForElement(FirstItemMark, timeout: TimeSpan.FromSeconds(5));
		}

		// Issue2680Test_ScrollEnabled (src\Compatibility\ControlGallery\src\Issues.Shared\Issue2680ScrollView.cs)
		[Test]
		public void ScrollEnabled()
		{
			this.IgnoreIfPlatforms(new[]
			{
				TestDevice.Android, TestDevice.iOS, TestDevice.Mac, TestDevice.Windows
			}, "https://github.com/dotnet/maui/pull/19181");
			
			App.WaitForElement(FirstItemMark);

			App.Click(ToggleButtonMark);

			// Waiting until merge https://github.com/dotnet/maui/pull/19181
			//App.ScrollDown(ScrollViewMark);
			//App.ScrollDown(ScrollViewMark);

			App.WaitForNoElement(FirstItemMark, timeout: TimeSpan.FromSeconds(5));
		}
	}
}