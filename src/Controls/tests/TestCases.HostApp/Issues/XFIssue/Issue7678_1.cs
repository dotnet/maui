using System;
using System.Collections.ObjectModel;

namespace Maui.Controls.Sample.Issues;


[Issue(IssueTracker.Github, 7678, "[Android] CarouselView binded to a new ObservableCollection filled with Items does not render content", PlatformAffected.Android, issueTestNumber: 1)]
public class Issue7678_2 : TestContentPage
{
	public Issue7678_2()
	{
		Title = "Issue 7678";
		BindingContext = new Issue7678ViewModel_1();
	}
	protected override void Init()
	{
		var layout = new StackLayout();

		var instructions = new Label
		{
			Margin = new Thickness(6),
			Text = "The CarouselView below must show items. If you do not see any item, this test has failed."
		};

		var itemsLayout =
			new LinearItemsLayout(ItemsLayoutOrientation.Horizontal)
			{
				SnapPointsType = SnapPointsType.MandatorySingle,
				SnapPointsAlignment = SnapPointsAlignment.Center
			};

		var carouselView = new CarouselView
		{
			AutomationId = "carouselView",
			ItemsLayout = itemsLayout,
			ItemTemplate = GetCarouselTemplate()
		};

		carouselView.SetBinding(ItemsView.ItemsSourceProperty, "Items");

		layout.Children.Add(instructions);
		layout.Children.Add(carouselView);

		Content = layout;
	}

	protected override async void OnAppearing()
	{
		base.OnAppearing();

		await ((Issue7678ViewModel_1)BindingContext).LoadItemsAsync();
	}

	internal DataTemplate GetCarouselTemplate()
	{
		return new DataTemplate(() =>
		{
			var grid = new Grid();

			var info = new Label
			{
				HorizontalOptions = LayoutOptions.Center,
				VerticalOptions = LayoutOptions.Center,
				Margin = new Thickness(6)
			};

			info.SetBinding(Label.TextProperty, new Binding("Name"));

			grid.Children.Add(info);

			var border = new Border
			{
				Content = grid
			};

			border.SetBinding(BackgroundColorProperty, new Binding("Color"));

			return border;
		});
	}
}

public class Issue7678Model_1
{
	public Color Color { get; set; }
	public string Name { get; set; }
}

public class Issue7678ViewModel_1 : BindableObject
{
	ObservableCollection<Issue7678Model_1> _items;

	public ObservableCollection<Issue7678Model_1> Items
	{
		get { return _items; }
		set
		{
			_items = value;
			OnPropertyChanged();
		}
	}

	public async Task LoadItemsAsync()
	{
		Items = new ObservableCollection<Issue7678Model_1>();

		var random = new Random();
		var items = new List<Issue7678Model_1>();

		for (int n = 0; n < 5; n++)
		{
			items.Add(new Issue7678Model_1
			{
				Color = Color.FromRgb(random.Next(0, 255), random.Next(0, 255), random.Next(0, 255)),
				Name = $"{n + 1}"
			});
		}

		await Task.Delay(500);

		_items = new ObservableCollection<Issue7678Model_1>(items);
		OnPropertyChanged(nameof(Items));
	}
}

