using System.Threading.Tasks;
using Microsoft.Maui.Controls;

namespace Controls.Sample.UITests
{
	public class ScrollViewNoContent : ContentPage
	{
		readonly Label _label;
		readonly ScrollView scrollView;

		public ScrollViewNoContent()
		{
			scrollView = new ScrollView();
			_label = new Label();

			Content = new StackLayout()
			{
				Children =
				{
					_label,
					scrollView
				}
			};
		}

		protected override async void OnAppearing()
		{
			base.OnAppearing();
			await Task.Delay(500);
			scrollView.Content = new StackLayout();
			await Task.Delay(500);
			scrollView.Content = null;
			await Task.Delay(500);
			_label.Text = "Success";
		}
	}
}
