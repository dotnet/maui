using Microsoft.Maui.Controls;
using Microsoft.Maui.Controls.Shapes;
using Microsoft.Maui.Graphics;

namespace Maui.Controls.Sample.Issues
{
	[Issue(IssueTracker.Github, 36942, "Border with Shadow breaks descendant BackgroundColor UI updates on Android", PlatformAffected.Android)]
	public class Issue36942 : ContentPage
	{
		static readonly Color ActivatedColor = Colors.DodgerBlue;
		static readonly Color DefaultColor = Color.FromArgb("#FFF5F5F5");

		readonly Border _toggleTarget;
		readonly Label _viewModelStateLabel;

		bool _activated;

		public Issue36942()
		{
			AutomationId = "Issue36942Page";
			Title = "Issue 36942";
			BackgroundColor = Colors.White;

			_toggleTarget = new Border
			{
				AutomationId = "ToggleTarget",
				Stroke = Colors.Transparent,
				StrokeShape = new RoundRectangle { CornerRadius = 20 },
				BackgroundColor = DefaultColor,
				Padding = new Thickness(20, 10),
				WidthRequest = 220,
				HorizontalOptions = LayoutOptions.Center,
				Content = new Label
				{
					Text = "Tap to toggle",
					TextColor = Color.FromArgb("#333333"),
					HorizontalOptions = LayoutOptions.Center,
					VerticalOptions = LayoutOptions.Center,
				},
			};

			var tap = new TapGestureRecognizer();
			tap.Tapped += OnToggleTapped;
			_toggleTarget.GestureRecognizers.Add(tap);

			var outerBorder = new Border
			{
				Stroke = Colors.Transparent,
				StrokeShape = new RoundRectangle { CornerRadius = 20 },
				BackgroundColor = Color.FromArgb("#222222"),
				Padding = 20,
				HorizontalOptions = LayoutOptions.Center,
				VerticalOptions = LayoutOptions.Center,
				Shadow = new Shadow
				{
					Brush = Brush.Black,
					Offset = new Point(20, 20),
					Radius = 40,
					Opacity = 0.8f,
				},
				Content = _toggleTarget,
			};

			_viewModelStateLabel = new Label
			{
				AutomationId = "ViewModelState",
				Text = "Activated: False",
				FontSize = 16,
				Margin = new Thickness(0, 40, 0, 0),
				HorizontalOptions = LayoutOptions.Center,
			};

			var grid = new Grid
			{
				Padding = 40,
				RowDefinitions =
				{
					new RowDefinition { Height = GridLength.Star },
					new RowDefinition { Height = GridLength.Auto },
					new RowDefinition { Height = GridLength.Auto },
					new RowDefinition { Height = GridLength.Star },
				},
			};
			grid.Add(outerBorder, 0, 1);
			grid.Add(_viewModelStateLabel, 0, 2);

			Content = grid;
		}
		void OnToggleTapped(object sender, TappedEventArgs e)
		{
			_activated = !_activated;
			_toggleTarget.BackgroundColor = _activated ? ActivatedColor : DefaultColor;
			_viewModelStateLabel.Text = $"Activated: {_activated}";
		}
	}
}
