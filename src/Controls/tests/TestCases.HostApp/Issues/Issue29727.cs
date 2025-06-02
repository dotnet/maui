using Microsoft.Maui.Controls.Shapes;

namespace Maui.Controls.Sample.Issues;

[Issue(IssueTracker.Github, 29727, "I1 - Vertical list for Item Height- After rotating the Android emulator, some text boxes have extra blank space", PlatformAffected.Android)]
public class Issue29727 : ContentPage
{
	public List<string> Items { get; set; }
	public Issue29727()
	{
		Title = "Consistent Height Test";

		Items = new List<string>
		{
			"If you're visiting this page, you're likely here because you're searching for a random sentence.",
			"Sometimes a random word just isn't enough, and that is where the random sentence generator comes into play. By inputting the desired number, you can make a list of as many random sentences as you want or need. Producing random sentences can be helpful in a number of different ways.",
			"For writers, a random sentence can help them get their creative juices flowing. Since the topic of the sentence is completely unknown, it forces the writer to be creative when the sentence appears. There are a number of different ways a writer can use the random sentence for creativity. The most common way to use the sentence is to begin a story. Another option is to include it somewhere in the story. A much more difficult challenge is to use it to end a story. In any of these cases, it forces the writer to think creatively since they have no idea what sentence will appear from the tool.",
			"For those writers who have writers' block, this can be an excellent way to take a step to crumbling those walls.",
			"It can also be successfully used as a daily exercise to get writers to begin writing. Being shown a random sentence and using it to complete a paragraph each day can be an excellent way to begin any writing session.",
			"By taking the writer away from the subject matter that is causing the block, a random sentence may allow them to see the project they're working on in a different light and perspective. Sometimes all it takes is to get that first sentence down to help break the block.",
			"It can also be a fun way to surprise others. You might choose to share a random sentence on social media just to see what type of reaction it garners from others. It's an unexpected move that might create more conversation than a typical post or tweet.",
			"Have several random sentences generated and you'll soon be able to see if they can help with your project."
		};
		BindingContext = this;

		var mainGrid = new Grid
		{
			Margin = new Thickness(20),
			RowDefinitions =
			{
				new RowDefinition { Height = GridLength.Auto },
				new RowDefinition { Height = GridLength.Auto },
				new RowDefinition { Height = GridLength.Star }
			}
		};

		var instructionLabel = new Label
		{
			Text = "1. The test pass if the item heights are consistent when scrolling.",
		};

		var headerLayout = new StackLayout();
		headerLayout.Children.Add(instructionLabel);
		mainGrid.Children.Add(headerLayout);
		Grid.SetRow(headerLayout, 0);

		var collectionView = new CollectionView
		{
			ItemsSource = Items,
			AutomationId = "TestCollectionView",
			ItemTemplate = new DataTemplate(() =>
			{
				var outerStack = new VerticalStackLayout { Padding = 20 };

				var border = new Border
				{
					StrokeShape = new RoundRectangle { CornerRadius = 15 },
					BackgroundColor = Colors.Red,
					Padding = 10
				};

				var grid = new Grid
				{
					ColumnDefinitions =
					{
						new ColumnDefinition { Width = 40 },
						new ColumnDefinition { Width = GridLength.Star }
					},
					ColumnSpacing = 10
				};

				var imageStack = new VerticalStackLayout
				{
					VerticalOptions = LayoutOptions.Start,
					Spacing = 5
				};

				var image = new Image
				{
					Source = "dotnet_bot.png",
					WidthRequest = 40,
					HeightRequest = 40,
					VerticalOptions = LayoutOptions.Start,
					HorizontalOptions = LayoutOptions.Center
				};

				imageStack.Children.Add(image);
				grid.Children.Add(imageStack);
				Grid.SetColumn(imageStack, 0);

				var textStack = new VerticalStackLayout();

				var labelGrid = new Grid { Margin = new Thickness(0, 0, 0, 10) };
				labelGrid.Children.Add(new Label
				{
					Text = "Username",
					HorizontalOptions = LayoutOptions.Start
				});
				labelGrid.Children.Add(new Label
				{
					Text = "Today",
					VerticalOptions = LayoutOptions.Center,
					HorizontalOptions = LayoutOptions.End
				});

				var contentLabel = new Label
				{
					Margin = new Thickness(0, 0, 0, 10)
				};
				contentLabel.SetBinding(Label.TextProperty, ".");

				textStack.Children.Add(labelGrid);
				textStack.Children.Add(contentLabel);

				grid.Children.Add(textStack);
				grid.SetColumn(textStack, 1);
				border.Content = grid;
				outerStack.Children.Add(border);

				return outerStack;
			})
		};

		var button = new Button
		{
			AutomationId = "ScrollToDownButton",
			Text = "Scroll to Item 4",
			HorizontalOptions = LayoutOptions.Center,
			Margin = new Thickness(0, 10, 0, 10)
		};

		button.Clicked += (s, e) =>
		{
			collectionView.ScrollTo(5, position: ScrollToPosition.End, animate: true);
		};
		mainGrid.Children.Add(button);
		Grid.SetRow(button, 1);
		mainGrid.Children.Add(collectionView);
		Grid.SetRow(collectionView, 2);

		Content = mainGrid;
	}
}