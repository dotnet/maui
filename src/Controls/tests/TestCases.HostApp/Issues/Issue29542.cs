namespace Maui.Controls.Sample.Issues;

[Issue(IssueTracker.Github, 29542, "I1_Vertical_list_for_Multiple_Rows - Rotating the emulator would cause clipping on the description text.", PlatformAffected.Android)]
public class Issue29542 : ContentPage
{
	public Issue29542()
	{
		var grid = new Grid
		{
			Margin = 20,
			RowDefinitions =
			{
				new RowDefinition { Height = GridLength.Auto },
				new RowDefinition { Height = GridLength.Auto },
				new RowDefinition { Height = GridLength.Star },

			}
		};

		// Top text description
		var headerLabel = new Label
		{
			Text = "1. The test passes if resizing or rotating without clipping or elements disappearing in multiple rows.",
		};

		var button = new Button
		{
			Text = "Scroll Down",
			AutomationId = "ScrollToDownButton",
			BackgroundColor = Colors.Red,
			HorizontalOptions = LayoutOptions.Center,
			VerticalOptions = LayoutOptions.Center,
		};

		var verticalHeaderLayout = new VerticalStackLayout()
		{
			Children =
			{
				headerLabel
			},
		};
		Grid.SetRow(verticalHeaderLayout, 0);
		Grid.SetRow(button, 1);
		var items = new List<string>();
		for (int i = 1; i <= 21; i++)
		{
			items.Add(i.ToString());
		}
		var myCollection = new CollectionView
		{
			AutomationId = "TestCollectionView",
			ItemsSource = items,
			ItemsLayout = new GridItemsLayout(1, ItemsLayoutOrientation.Vertical)
			{
				VerticalItemSpacing = 5
			},
			Header = new Label
			{
				Text = "Header",
				FontSize = 32,
				TextColor = Color.FromArgb("#3B3A39")
			},
			ItemTemplate = new DataTemplate(CreateItemTemplate)
		};

		Grid.SetRow(myCollection, 2);
		button.Clicked += (s, e) =>
		{

			myCollection.ScrollTo(15, position: ScrollToPosition.End, animate: true);
		};

		// Add to grid
		grid.Children.Add(verticalHeaderLayout);
		grid.Children.Add(button);

		grid.Children.Add(myCollection);
		Content = grid;
	}

	private View CreateItemTemplate()
	{
		var label = new Label
		{
			MaxLines = 3,
			LineBreakMode = LineBreakMode.TailTruncation,
			Text = @"DescriptionDescriptionDescriptionDescriptionDescriptionDescriptionDescriptionDescriptionDescriptionDescriptionDescriptionDescription DescriptionDescriptionDescriptionDescriptionDescriptionDescriptionDescriptionDescription 
				   DescriptionDescriptionDescriptionDescriptionDescriptionDescription DescriptionDescriptionDescriptionDescriptionDescriptionDescriptionDescriptionDescriptionDescriptionDescriptionDescriptionDescriptionDescriptionDescriptionDescription
				   DescriptionDescriptionDescription
				   DescriptionDescriptionDescription",
			BackgroundColor = Colors.Green,
			HorizontalOptions = LayoutOptions.Start
		};

		label.SetBinding(AutomationIdProperty, new Binding(".", stringFormat: "Label_{0}"));

		var image = new Image
		{
			Aspect = Aspect.AspectFit,
			BackgroundColor = Colors.Transparent,
			VerticalOptions = LayoutOptions.Center,
			HorizontalOptions = LayoutOptions.Center,
			HeightRequest = 24,
			Source = "dotnet_bot.png",
			Margin = new Thickness(0, 0, 20, 0)
		};

		var layout = new Grid
		{
			ColumnDefinitions =
		{
			new ColumnDefinition { Width = GridLength.Star },
			new ColumnDefinition { Width = GridLength.Auto }
		},
			BackgroundColor = Colors.AliceBlue,
			Padding = 5
		};

		layout.Children.Add(label);
		layout.Children.Add(image);
		Grid.SetColumn(label, 0);
		Grid.SetColumn(image, 1);

		return layout;
	}
}