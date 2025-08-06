using System.Collections.ObjectModel;

namespace Maui.Controls.Sample.Issues;
[Issue(IssueTracker.Github, 23702, "CollectionView with GridItemsLayout (Span=1) doesn't adapt to window width reduction on Windows platform", PlatformAffected.WinRT)]
public class Issue23702 : ContentPage
{
	private Label _widthLabel;

	public Issue23702()
	{
		var titleLabel = new Label
		{
			Text = "Label width after dynamically changing width to collectionview:",
			FontAttributes = FontAttributes.Bold,
			Margin = new Thickness(10, 10, 10, 0)
		};

		_widthLabel = new Label
		{
			AutomationId = "WidthLabel",
			Text = "0",
			FontAttributes = FontAttributes.Bold,
			BackgroundColor = Colors.Yellow,
			Padding = new Thickness(10),
			HorizontalOptions = LayoutOptions.FillAndExpand
		};

		var labelContainer = new Border
		{
			Content = _widthLabel,
			BackgroundColor = Colors.White,
			Stroke = Colors.Black,
			StrokeThickness = 1,
			Padding = new Thickness(5),
			Margin = new Thickness(10)
		};

		var collectionView = new CollectionView
		{
			AutomationId = "TestCollectionView",
			BackgroundColor = Colors.LightBlue,
			ItemsSource = new ObservableCollection<Issue23702Item>
			{
				new Issue23702Item { Name = "Item 1" },
				new Issue23702Item { Name = "Item 2" },
				new Issue23702Item { Name = "Item 3" },
				new Issue23702Item { Name = "Item 4" },
				new Issue23702Item { Name = "Item 5" }
			},
			ItemsLayout = new GridItemsLayout(1, ItemsLayoutOrientation.Vertical),
			ItemTemplate = new DataTemplate(() =>
			{
				var stackLayout = new StackLayout 
				{ 
					HeightRequest = 80,
					HorizontalOptions = LayoutOptions.FillAndExpand,
					Orientation = StackOrientation.Horizontal,
					Padding = new Thickness(5)
				};

				var label = new Label
				{
					BackgroundColor = Colors.LightGray,
					HorizontalOptions = LayoutOptions.FillAndExpand,
					VerticalOptions = LayoutOptions.CenterAndExpand
				};
				label.SetBinding(Label.TextProperty, "Name");
				
				var border = new Border
				{
					Stroke = Colors.Red,
					StrokeThickness = 2,
					Content = label,
					HorizontalOptions = LayoutOptions.FillAndExpand
				};

				label.PropertyChanged += (sender, e) =>
				{
					if (e.PropertyName == "Width" && sender is Label sl && sl.Width > 0)
					{
						_widthLabel.Text = Math.Round(sl.Width).ToString();
					}
				};
				
				stackLayout.Children.Add(border);
				return stackLayout;
			})
		};

		var setWidthButton = new Button
		{
			AutomationId = "SetWidthButton",
			Text = "Set Width to 100",
			Command = new Command(() =>
			{
				collectionView.WidthRequest = 100;
			})
		};

		Content = new StackLayout
		{
			Children = { titleLabel, labelContainer, setWidthButton, collectionView }
		};
	}
}

public class Issue23702Item
{
	public string Name { get; set; } = string.Empty;
}
