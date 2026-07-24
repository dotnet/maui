namespace Maui.Controls.Sample.Issues;

[Issue(IssueTracker.Github, 35675, "[iOS] CarouselView freezes with infinite loop when IsScrollAnimated=False", PlatformAffected.iOS)]
public class Issue35675 : ContentPage
{
	readonly string[] _initialItems = ["Item 0", "Item 1", "Item 2"];
	readonly string[] _updatedItems = ["Item 0b", "Item 1b", "Item 2b"];

	public Issue35675()
	{
		Label instructionLabel = new Label
		{
			AutomationId = "InstructionLabel",
			Text = "The test passes if the CarouselView is not frozen after the button click and the current item is updated properly.",
			HorizontalOptions = LayoutOptions.Center,
			HorizontalTextAlignment = TextAlignment.Center,
			FontSize = 18
		};

		CarouselView carouselView = new CarouselView
		{
			AutomationId = "CarouselView",
			HeightRequest = 300,
			IsScrollAnimated = false,
			BackgroundColor = Colors.LightGray,
			HorizontalScrollBarVisibility = ScrollBarVisibility.Never,
			ItemsSource = _initialItems,
			ItemTemplate = new DataTemplate(() =>
			{
				Label label = new Label
				{
					FontSize = 24
				};
				label.SetBinding(Label.TextProperty, ".");
				label.SetBinding(Label.AutomationIdProperty, ".");
				return label;
			})
		};

		Button scrollButton = new Button
		{
			Text = "Change Items And Scroll",
			AutomationId = "ScrollButton"
		};

		scrollButton.Clicked += (s, e) =>
		{
			carouselView.ItemsSource = _updatedItems;
			carouselView.CurrentItem = "Item 2b";
		};

		Content = new VerticalStackLayout
		{
			Padding = 20,
			Spacing = 15,
			Children = { instructionLabel, carouselView, scrollButton }
		};
	}
}
