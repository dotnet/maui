using System.Collections.ObjectModel;
using Microsoft.Maui.Controls.Shapes;

namespace Maui.Controls.Sample.Issues;

[Issue(IssueTracker.Github, 35700,
	"Grouped CollectionView items not rendered properly on Android with GridItemsLayout",
	PlatformAffected.Android)]
public class Issue35700 : TestContentPage
{
	protected override void Init()
	{
		var collectionView = new CollectionView2
		{
			AutomationId = "TestCollectionView",
			IsGrouped = true,
			HorizontalOptions = LayoutOptions.Fill,
			Margin = new Thickness(5, 30, 5, 5),
			ItemsLayout = new GridItemsLayout(ItemsLayoutOrientation.Vertical)
			{
				Span = 5,
				VerticalItemSpacing = 10,
				HorizontalItemSpacing = 10,
			},
			GroupHeaderTemplate = new DataTemplate(() =>
			{
				var label = new Label
				{
					HorizontalOptions = LayoutOptions.Fill,
					HorizontalTextAlignment = TextAlignment.Start,
					Padding = new Thickness(10),
					FontSize = 18,
					TextColor = Colors.White,
					FontAttributes = FontAttributes.Bold,
					BackgroundColor = Colors.Gray,
				};
				label.SetBinding(Label.TextProperty, "Name");
				return label;
			}),
			ItemTemplate = new DataTemplate(() =>
			{
				var label = new Label
				{
					HorizontalOptions = LayoutOptions.Center,
					TextColor = Colors.White,
					VerticalOptions = LayoutOptions.Center,
					HorizontalTextAlignment = TextAlignment.Center,
				};
				label.SetBinding(Label.TextProperty, ".");

				return new Border
				{
					StrokeShape = new RoundRectangle { CornerRadius = 10 },
					Padding = new Thickness(5),
					MinimumWidthRequest = 50,
					Stroke = Colors.Transparent,
					BackgroundColor = Colors.Gray,
					StrokeThickness = 1,
					HorizontalOptions = LayoutOptions.Center,
					Content = label,
				};
			}),
		};

		collectionView.ItemsSource = new ObservableCollection<NumberGroup35700>
		{
			new NumberGroup35700("100s", new List<string>
			{
				"100", "200", "300", "400", "500",
				"600", "700", "800", "900",
			}),
			new NumberGroup35700("1000s", new List<string>
			{
				"1000", "2000", "3000", "4000", "5000",
				"6000", "7000", "8000", "9000",
			}),
		};

		Content = collectionView;
	}
}

public class NumberGroup35700 : ObservableCollection<string>
{
	public string Name { get; private set; }

	public NumberGroup35700(string name, List<string> numbers) : base(numbers)
	{
		Name = name;
	}
}
