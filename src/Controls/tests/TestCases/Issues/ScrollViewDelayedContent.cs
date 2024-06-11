using System.Threading.Tasks;
using Microsoft.Maui;
using Microsoft.Maui.Controls;

namespace Maui.Controls.Sample.Issues
{
	// MeasuringEmptyScrollViewDoesNotCrash (src\Compatibility\ControlGallery\src\Issues.Shared\Issue1538.cs)
	[Issue(IssueTracker.None, 1538, "Crash measuring empty ScrollView", PlatformAffected.Android | PlatformAffected.UWP)]
	public class ScrollViewDelayedContent : ContentPage
	{
		readonly ScrollView _sv;

		public ScrollViewDelayedContent()
		{
			Title = "ScrollView delayed Content";

			StackLayout sl = new StackLayout() { VerticalOptions = LayoutOptions.Fill };
			sl.Children.Add(_sv = new ScrollView() { HeightRequest = 100 });
			Content = sl;

			AddContentDelayed();
		}

		async void AddContentDelayed()
		{
			await Task.Delay(1000);
			_sv.Content = new Label { Text = "Foo" };
		}
	}
}