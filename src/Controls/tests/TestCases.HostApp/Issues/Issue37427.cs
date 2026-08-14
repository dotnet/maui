using System.Collections.ObjectModel;

namespace Maui.Controls.Sample.Issues;

[Issue(IssueTracker.Github, 37427, "CollectionView item content renders with zero width on iOS", PlatformAffected.iOS | PlatformAffected.macOS)]
public class Issue37427 : ContentPage
{
	readonly CollectionView _collectionView;

	public Issue37427()
	{
		Title = "Issue 37427";

		var scrollButton = new Button
		{
			AutomationId = "37427ScrollButton",
			Text = "Scroll to item 15"
		};
		scrollButton.Clicked += (_, _) => _collectionView.ScrollTo(15, position: ScrollToPosition.Start, animate: false);

		_collectionView = new CollectionView
		{
			AutomationId = "37427CollectionView",
			ItemsSource = CreateItems(),
			ItemTemplate = new DataTemplate(() => new Issue37427Card())
		};

		Content = new Grid
		{
			RowDefinitions =
			{
				new RowDefinition(GridLength.Auto),
				new RowDefinition(GridLength.Star)
			},
			Children =
			{
				scrollButton,
				_collectionView
			}
		};

		Grid.SetRow(_collectionView, 1);
	}

	static ObservableCollection<Issue37427Item> CreateItems()
	{
		var items = new ObservableCollection<Issue37427Item>();
		for (var index = 0; index < 20; index++)
		{
			items.Add(new(
				index,
				[
					Color.FromRgb((index * 37 + 180) % 255, 30, 30),
					Color.FromRgb(255, (index * 41 + 80) % 255, 20),
					Color.FromRgb(240, 210, (index * 29) % 255),
					Color.FromRgb(20, (index * 31 + 100) % 255, 40),
					Color.FromRgb(20, 40, (index * 43 + 120) % 255)
				]));
		}

		return items;
	}
}

sealed class Issue37427Card : Border
{
	public static readonly BindableProperty PreviewColorsProperty = BindableProperty.Create(
		nameof(PreviewColors),
		typeof(IEnumerable<Color>),
		typeof(Issue37427Card),
		propertyChanged: OnPreviewColorsChanged);

	readonly Grid _preview;

	public Issue37427Card()
	{
		Margin = new Thickness(16, 0, 16, 16);
		Stroke = Colors.LightGray;

		var image = new Image
		{
			Aspect = Aspect.AspectFit,
			HeightRequest = 50,
			Source = "dotnet_bot.png"
		};

		_preview = new Grid
		{
			ColumnSpacing = 0,
			HeightRequest = 32,
			HorizontalOptions = LayoutOptions.Fill,
			VerticalOptions = LayoutOptions.Fill
		};

		var previewBorder = new Border
		{
			Content = _preview,
			HorizontalOptions = LayoutOptions.Fill,
			Margin = new Thickness(12, 8)
		};

		var label = new Label { Margin = new Thickness(12, 0, 12, 12) };
		label.SetBinding(Label.TextProperty, nameof(Issue37427Item.Title));
		label.SetBinding(Label.AutomationIdProperty, nameof(Issue37427Item.AutomationId));

		var content = new Grid
		{
			RowDefinitions =
			{
				new RowDefinition(GridLength.Auto),
				new RowDefinition(GridLength.Auto),
				new RowDefinition(GridLength.Auto)
			},
			Children =
			{
				image,
				previewBorder,
				label
			}
		};

		Grid.SetRow(previewBorder, 1);
		Grid.SetRow(label, 2);
		Content = content;

		SetBinding(PreviewColorsProperty, new Binding(nameof(Issue37427Item.PreviewColors)));
	}

	public IEnumerable<Color> PreviewColors
	{
		get => (IEnumerable<Color>)GetValue(PreviewColorsProperty);
		set => SetValue(PreviewColorsProperty, value);
	}

	static void OnPreviewColorsChanged(BindableObject bindable, object oldValue, object newValue)
	{
		if (bindable is not Issue37427Card card || newValue is not IEnumerable<Color> colors)
		{
			return;
		}

		card._preview.ColumnDefinitions.Clear();
		card._preview.Children.Clear();

		var column = 0;
		foreach (var color in colors)
		{
			card._preview.ColumnDefinitions.Add(new ColumnDefinition(GridLength.Star));
			card._preview.Add(new BoxView { BackgroundColor = color, HeightRequest = 32 }, column++);
		}
	}
}

sealed record Issue37427Item(int Index, IEnumerable<Color> PreviewColors)
{
	public string Title => $"Card {Index}";

	public string AutomationId => $"37427Card{Index}";
}
