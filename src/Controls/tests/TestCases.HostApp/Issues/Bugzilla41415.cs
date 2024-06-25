using System;
using Microsoft.Maui;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Graphics;

namespace Maui.Controls.Sample.Issues
{
	[Issue(IssueTracker.None, 41415, "ScrollX and ScrollY values are not consistent with iOS", PlatformAffected.Android)]
	public class Bugzilla41415 : ContentPage
	{
		const string ButtonId = "ClickId";
		const string ButtonText = "Click Me";

		float _x;
		bool _didXChange, _didYChange;

		public Bugzilla41415()
		{
			var grid = new Grid
			{
				BackgroundColor = Colors.Yellow,
				WidthRequest = 1000,
				HeightRequest = 1000,
				Children =
				{
					new BoxView
					{
						WidthRequest =  200,
						HeightRequest = 200,
						BackgroundColor = Colors.Red,
						HorizontalOptions = LayoutOptions.Center,
						VerticalOptions = LayoutOptions.Center
					}
				}
			};

			var labelx = new Label();
			var labely = new Label();
			var labelz = new Label();
			var labela = new Label();

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

				// first and second taps
				if (_x == 0)
				{
					if (Math.Round(args.ScrollX) != 0 && Math.Round(args.ScrollX) != 100)
						_didXChange = true;
					if (Math.Round(args.ScrollY) != 0 && Math.Round(args.ScrollY) != 100)
						_didYChange = true;
				}
				else if (_x == 100)
				{
					if (Math.Round(args.ScrollX) != _x && Math.Round(args.ScrollX) != _x + 100)
						_didXChange = true;
					if (Math.Round(args.ScrollY) != 100)
						_didYChange = true;
				}

				labelz.Text = "z: " + _didXChange.ToString();
				labela.Text = "a: " + _didYChange.ToString();
			};

			var button = new Button { AutomationId = ButtonId, Text = ButtonText };
			button.Clicked += async (sender, e) =>
			{
				// reset
				labelx.Text = null;
				labely.Text = null;
				labelz.Text = null;
				labela.Text = null;
				_didXChange = false;
				_didYChange = false;

				await scrollView.ScrollToAsync(_x + 100, 100, true);
				_x = 100;
			};

			Content = new StackLayout { Children = { button, labelx, labely, labelz, labela, scrollView } };
		}
	}
}