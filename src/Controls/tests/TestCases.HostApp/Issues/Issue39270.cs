using System.Collections.ObjectModel;

namespace Maui.Controls.Sample.Issues;

[Issue(IssueTracker.Github, 39270, "CollectionView items disappear after device rotation", PlatformAffected.iOS)]
public class Issue39270 : ContentPage
{
	public Issue39270()
	{
		var grid = new Grid
		{
			RowDefinitions =
			{
				new RowDefinition { Height = GridLength.Auto },
				new RowDefinition { Height = GridLength.Auto },
				new RowDefinition { Height = GridLength.Auto },
				new RowDefinition { Height = GridLength.Auto }
			}
		};

		grid.Add(new Label
		{
			Text = "1. The test passes if all CollectionView items remain visible after changing the device orientation.",
			AutomationId = "label"
		}, 0, 0);

		var collectionView = new CollectionView2
		{
			ItemsLayout = new LinearItemsLayout(ItemsLayoutOrientation.Horizontal),
			HeightRequest = 200,
			ItemTemplate = new DataTemplate(() =>
			{
				var item = new Grid
				{
					HeightRequest = 200
				};

				item.SetBinding(Grid.BackgroundColorProperty, "Color");
				item.SetBinding(Grid.WidthRequestProperty, "Width");

				var label = new Label
				{
					TextColor = Colors.Black
				};

				label.SetBinding(Label.TextProperty, "Index");
				item.Add(label);

				return item;
			})
		};

		collectionView.SetBinding(ItemsView.ItemsSourceProperty, "Items");

		grid.Add(collectionView, 0, 1);

		grid.Add(new Label
		{
			Text = "2. The test passes if the CollectionView items remain visible after the layout changes."
		}, 0, 2);

		var collectionView2 = new CollectionView2
		{
			ItemsLayout = new LinearItemsLayout(ItemsLayoutOrientation.Horizontal),
			ItemSizingStrategy = ItemSizingStrategy.MeasureAllItems,
			HeightRequest = 200,
			ItemTemplate = new DataTemplate(() =>
			{
				var item = new Grid
				{
					HeightRequest = 200
				};

				item.SetBinding(Grid.BackgroundColorProperty, "Color");
				item.SetBinding(Grid.WidthRequestProperty, "Width");

				var label = new Label
				{
					TextColor = Colors.Black
				};

				label.SetBinding(Label.TextProperty, "Index");
				item.Add(label);

				return item;
			})
		};

		collectionView2.SetBinding(ItemsView.ItemsSourceProperty, "Items2");

		grid.Add(collectionView2, 0, 3);

		Content = grid;
		BindingContext = new Issue39270ViewModel();
	}
}

public class Issue39270ViewModel
{
	public ObservableCollection<Issue39270Model> Items { get; } = new();
	public ObservableCollection<Issue39270Model> Items2 { get; } = new();

	public Issue39270ViewModel()
	{
		Items.Add(new Issue39270Model { Index = 0, Color = GetColor(0), Width = 100 });

		for (int i = 1; i < 16; i++)
			Items.Add(new Issue39270Model { Index = i, Color = GetColor(i), Width = 50 });

		Items.Add(new Issue39270Model { Index = 16, Color = GetColor(17), Width = 100 });
		Items.Add(new Issue39270Model { Index = 17, Color = GetColor(18), Width = 100 });

		Items2.Add(new Issue39270Model { Index = 0, Color = GetColor(0), Width = 50 });

		for (int i = 1; i < 16; i++)
			Items2.Add(new Issue39270Model { Index = i, Color = GetColor(i + 1), Width = 50 });

		Items2.Add(new Issue39270Model { Index = 16, Color = GetColor(17), Width = 100 });
		Items2.Add(new Issue39270Model { Index = 17, Color = GetColor(18), Width = 100 });
	}

	Color GetColor(int i)
	{
		switch (i % 4)
		{
			case 0:
				return Color.FromRgb(90, 140, 115);
			case 1:
				return Color.FromRgb(243, 226, 148);
			case 2:
				return Color.FromRgb(240, 175, 115);
			case 3:
				return Color.FromRgb(217, 100, 90);
			default:
				return Color.FromRgb(0, 0, 0);
		}
	}
}

public class Issue39270Model
{
	public Color Color { get; set; }
	public int Width { get; set; }
	public int Index { get; set; }
}
