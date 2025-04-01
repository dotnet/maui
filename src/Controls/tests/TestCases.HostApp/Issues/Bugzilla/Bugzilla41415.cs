namespace Maui.Controls.Sample.Issues
{
	[Issue(IssueTracker.None, 41415, "ScrollX and ScrollY values at the ScrollView.Scrolled event are not consistent in ScrollOrientation.Both mode", PlatformAffected.Android)]
	public class Bugzilla41415 : ContentPage
	{
		const string ButtonId = "ClickId";
		const string ButtonText = "Click Me";

		float _x;
		public Bugzilla41415()
		{
			var grid = new Grid
			{
				BackgroundColor = Colors.Yellow,
				// To make this test works for desktop apps, we need to set a big size for the grid
				WidthRequest = 3000,
				HeightRequest = 3000,
				Children =
				{
					new BoxView
					{
						WidthRequest =  2200,
						HeightRequest = 2200,
						BackgroundColor = Colors.Red,
						HorizontalOptions = LayoutOptions.Center,
						VerticalOptions = LayoutOptions.Center
					}
				}
			};

			var labelx = new Label();
			var labely = new Label();

			var scrollView = new ScrollView
			{
				Orientation = ScrollOrientation.Both,
				HorizontalOptions = LayoutOptions.Fill,
				VerticalOptions = LayoutOptions.Fill
			};

			scrollView.Content = grid;
			scrollView.Scrolled += (sender, args) =>
			{
				labelx.Text = $"x: {(int)Math.Round(args.ScrollX)}";
				labely.Text = $"y: {(int)Math.Round(args.ScrollY)}";
			};

			var button = new Button { AutomationId = ButtonId, Text = ButtonText };
			button.Clicked += async (sender, e) =>
			{
				// reset
				labelx.Text = null;
				labely.Text = null;

				await scrollView.ScrollToAsync(_x + 100, 100, true);
				_x = 100;
			};

			Grid.SetRow(button, 0);
			Grid.SetRow(labelx, 1);
			Grid.SetRow(labely, 2);
			Grid.SetRow(scrollView, 3);

			Content = new Grid
			{
				RowDefinitions = new RowDefinitionCollection
				{
					new RowDefinition { Height = GridLength.Auto },
					new RowDefinition { Height = GridLength.Auto },
					new RowDefinition { Height = GridLength.Auto },
					new RowDefinition { Height = GridLength.Star },
				},
				Children =
				{
					button,
					labelx,
					labely,
					scrollView,
				}
			};
		}
	}
}