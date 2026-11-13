using Microsoft.Maui.Controls.Shapes;

namespace Maui.Controls.Sample.Issues;

[Issue(IssueTracker.Github, 33308, "CarouselView vertical MandatorySingle snap points should settle to exactly one item per swipe on iOS", PlatformAffected.iOS)]
public class Issue33308 : ContentPage
{
	public Issue33308()
	{
		var cards = Enumerable
			.Range(1, 10)
			.Select(index => new Issue33308CardItem($"Card {index}"))
			.ToList();

		var cardsCarousel = new CarouselView
		{
			Loop = false,
			AutomationId = "CardsCarousel",
			ItemsSource = cards,
			VerticalScrollBarVisibility = ScrollBarVisibility.Never,
			ItemsLayout = new LinearItemsLayout(ItemsLayoutOrientation.Vertical)
			{
				SnapPointsType = SnapPointsType.MandatorySingle,
				SnapPointsAlignment = SnapPointsAlignment.Center
			},
			ItemTemplate = new DataTemplate(() =>
			{
				var titleLabel = new Label
				{
					FontSize = 36,
					FontAttributes = FontAttributes.Bold,
					HorizontalOptions = LayoutOptions.Center,
					VerticalOptions = LayoutOptions.Center,
					TextColor = Color.FromArgb("#1E1E1E")
				};
				titleLabel.SetBinding(Label.TextProperty, nameof(Issue33308CardItem.Title));

				return new Grid
				{
					Padding = 16,
					Children =
					{
						new Border
						{
							Stroke = Color.FromArgb("#1E1E1E"),
							StrokeThickness = 1,
							BackgroundColor = Color.FromArgb("#F4F1EA"),
							StrokeShape = new RoundRectangle
							{
								CornerRadius = new CornerRadius(18)
							},
							Content = new Grid
							{
								Children =
								{
									titleLabel
								}
							}
						}
					}
				};
			})
		};

		Content = new Grid
		{
			Padding = 16,
			Children =
			{
				cardsCarousel
			}
		};
	}
}

public class Issue33308CardItem
{
	public Issue33308CardItem(string title)
	{
		Title = title;
	}

	public string Title { get; }
}
