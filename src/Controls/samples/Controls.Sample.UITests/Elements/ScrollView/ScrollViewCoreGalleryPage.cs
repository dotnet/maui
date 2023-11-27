using Microsoft.Maui;
using Microsoft.Maui.Controls;

namespace Controls.Sample.UITests
{
	class ScrollViewCoreGalleryPage : ContentPage
	{
		public ScrollViewCoreGalleryPage()
		{
			var descriptionLabel =
				   new Label { AutomationId = "WaitForStubControl", Text = "ScrollView Galleries", Margin = new Thickness(2) };

			Title = "ScrollView Galleries";

			Content = new ScrollView
			{
				Content = new StackLayout
				{
					Padding = new Thickness(2),
					Children =
					{
						descriptionLabel,
						// ScrollToYTwice (src\Compatibility\ControlGallery\src\UITests.Shared\Tests\ScrollViewUITests.cs)
						TestBuilder.NavButton("ScrollView ScrollTo", () =>
							new ScrollTo(), Navigation),
						TestBuilder.NavButton("ScrollView No Content", () =>
						// NullContentOnScrollViewDoesntCrash (src\Compatibility\ControlGallery\src\Issues.Shared\Issue3507.cs)
							new ScrollViewNoContent(), Navigation),
						// ScrollViewObjectDisposedTest (src\Compatibility\ControlGallery\src\Issues.Shared\ScrollViewObjectDisposed.cs)
						TestBuilder.NavButton("ScrollView No ObjectDisposed", () =>
							new ScrollViewObjectDisposed(), Navigation),
					}
				}
			};
		}
	}
}