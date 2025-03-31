namespace Maui.Controls.Sample.Issues
{
	[Issue(IssueTracker.Github, 27223, "SizeChanged event fires when size hasn't changed", PlatformAffected.UWP)]
	public class Issue27223 : ContentPage
	{
		private int counter = 0;
		private Label Counter;
		private Editor ShowSizes;
		private Editor ShowCoordinates;
		private VerticalStackLayout stackLayout;

		public Issue27223()
		{
			Grid mainGrid = new Grid
			{
				ColumnDefinitions =
				{
					new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) },
					new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) },
					new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) }
				}
			};

			Grid leftGrid = new Grid
			{
				RowDefinitions =
				{
					new RowDefinition { Height = GridLength.Auto },
					new RowDefinition { Height = new GridLength(1, GridUnitType.Star) },
					new RowDefinition { Height = GridLength.Auto }
				}
			};

			Counter = new Label { AutomationId = "Label" };
			leftGrid.Children.Add(Counter);
			Grid.SetRow(Counter, 0);

			stackLayout = new VerticalStackLayout();
			stackLayout.SizeChanged += VerticalStackLayout_SizeChanged;
			var button1 = new Button { Text = "button 1", AutomationId = "Button1" };
			button1.Clicked += Button_Clicked;
			stackLayout.Children.Add(button1);
			stackLayout.Children.Add(new Button { Text = "button 2" });
			stackLayout.Children.Add(new Button { Text = "button 3" });
			leftGrid.Children.Add(stackLayout);
			Grid.SetRow(stackLayout, 2);

			ShowSizes = new Editor { BackgroundColor = Colors.SkyBlue, VerticalOptions = LayoutOptions.Fill };
			ShowCoordinates = new Editor { BackgroundColor = Colors.DarkGoldenrod, VerticalOptions = LayoutOptions.Fill };

			mainGrid.Children.Add(leftGrid);
			Grid.SetColumn(leftGrid, 0);

			mainGrid.Children.Add(ShowSizes);
			Grid.SetColumn(ShowSizes, 1);

			mainGrid.Children.Add(ShowCoordinates);
			Grid.SetColumn(ShowCoordinates, 2);

			Content = mainGrid;
		}

		private void Button_Clicked(object sender, EventArgs e)
		{
#if WINDOWS
			Window.Height = 500;
#endif
		}

		private void VerticalStackLayout_SizeChanged(object sender, EventArgs e)
		{
			if (sender is VerticalStackLayout ctrl)
			{
				counter++;
				Counter.Text = $"resized {counter} times";
			}
		}
	}
}