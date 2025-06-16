using System.Collections.ObjectModel;

namespace Controls.TestCases.HostApp.Issues;

[Issue(IssueTracker.Github, 29772, "ItemSpacing doesnot work on CarouselView", PlatformAffected.UWP)]
public class Issue29772 : ContentPage
{
	Issue29772_ViewModel ViewModel;
	CarouselView carouselView;
	public Issue29772()
	{
		ViewModel = new Issue29772_ViewModel();
		BindingContext = ViewModel;
		var stack = new StackLayout();

		var descriptionlabel = new Label
		{
			Text = "ItemSpacing Should apply between items on CarouselView",
			HorizontalTextAlignment = TextAlignment.Center,
			AutomationId = "29772Descriptionlabel"
		};

		carouselView = new CarouselView
		{
			BackgroundColor = Colors.Red,
			HeightRequest = 400,
			WidthRequest = 300,
			ItemsSource = ViewModel.Items,
			Loop = false,
			CurrentItem = "Item 0",
			PeekAreaInsets = new Thickness(20, 0, 20, 0),
			ItemsLayout = new LinearItemsLayout(ItemsLayoutOrientation.Horizontal)
			{
				ItemSpacing = 10,
				SnapPointsAlignment = SnapPointsAlignment.Center,
				SnapPointsType = SnapPointsType.MandatorySingle,
			},

			ItemTemplate = new DataTemplate(() =>
			{
				var grid = new Grid
				{
					Margin = 0,
					Padding = 0,
					BackgroundColor = Colors.Yellow
				};

				var label = new Label
				{
					FontSize = 24,
					HorizontalOptions = LayoutOptions.Center,
					VerticalOptions = LayoutOptions.Center
				};
				label.SetBinding(Label.TextProperty, ".");

				grid.Children.Add(label);
				return grid;
			})
		};

		stack.Children.Add(descriptionlabel);
		stack.Children.Add(carouselView);
		Content = stack;
	}
}

public class Issue29772_ViewModel
{
	public ObservableCollection<string> Items { get; set; }

	public Issue29772_ViewModel()
	{
		Items = new ObservableCollection<string>();
		for (var i = 0; i < 4; i++)
		{
			Items.Add($"Item {i}");
		}
	}
}