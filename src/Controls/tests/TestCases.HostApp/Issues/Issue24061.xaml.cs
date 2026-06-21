namespace Maui.Controls.Sample.Issues
{
	[Issue(IssueTracker.Github, 24061, "ScrollView's gradient background doesn't work with ScrollView", PlatformAffected.iOS)]
	public partial class Issue24061 : ContentPage
	{
		LinearGradientBrush _linearGradientBrush;

		public Issue24061()
		{
			InitializeComponent();
			_linearGradientBrush = new LinearGradientBrush
			{
				StartPoint = new Point(0, 0.5),
				EndPoint = new Point(1, 0.5),
				GradientStops = new GradientStopCollection
					{
						new GradientStop { Color = Colors.White, Offset = 0.75f },
						new GradientStop { Color = Colors.Green, Offset = 1 }
					}
			};
			scrollView.Background = _linearGradientBrush;
		}

		private void Button_Clicked(object sender, EventArgs e)
		{
			if (scrollView.Background is LinearGradientBrush)
			{
				// Setting background to null will not reset to color on Android, Refer: https://github.com/dotnet/maui/issues/22914
				//scrollView.Background = null;
				scrollView.Background = new SolidColorBrush(Colors.White);
			}

			else
				scrollView.Background = _linearGradientBrush;
		}

		private async void ScrollButton_Clicked(object sender, EventArgs e)
		{
			// Scroll to the end of the horizontal ScrollView
			await scrollView.ScrollToAsync(scrollView.ContentSize.Width, 0, false);
		}
	}
}