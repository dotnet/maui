using System.Threading.Tasks;
using Microsoft.Maui.Controls;

namespace Maui.Controls.Sample.Issues
{
	// NullContentOnScrollViewDoesntCrash (src\Compatibility\ControlGallery\src\Issues.Shared\Issue3507.cs)
	[Issue(IssueTracker.None, 0, "Scrollview with null content crashes on Windows", PlatformAffected.UWP)]
	public class ScrollViewNoContent : ContentPage
	{
		readonly Label _label;
		readonly ScrollView _scrollView;

		public ScrollViewNoContent()
		{
			_scrollView = new ScrollView();
			_label = new Label();

			Content = new StackLayout()
			{
				Children =
				{
					_label,
					_scrollView
				}
			};
		}

		protected override async void OnAppearing()
		{
			base.OnAppearing();
			await Task.Delay(500);
			_scrollView.Content = new StackLayout();
			await Task.Delay(500);
			_scrollView.Content = null;
			await Task.Delay(500);
			_label.Text = "Success";
		}
	}
}
