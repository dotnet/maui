using System.Collections.ObjectModel;

namespace Maui.Controls.Sample.Issues;

[Issue(IssueTracker.Github, 23704, "CollectionView items disappear after device rotation", PlatformAffected.iOS)]
public class Issue23704 : ContentPage
{
	public Issue23704()
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
			Text = "1. The test passes if you are able to see the last item index(17) and verify that index(17) remains visible after resizing the window.",
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
			Text = "2. The test passes if you are able to see the last item index(17) and verify that index(17) remains visible after resizing the window."
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
		BindingContext = new Issue23704ViewModel();
	}
}

public class Issue23704ViewModel
{
	public ObservableCollection<Issue23704Model> Items { get; } = new();
	public ObservableCollection<Issue23704Model> Items2 { get; } = new();

	public Issue23704ViewModel()
	{
		Items.Add(new Issue23704Model { Index = 0, Color = GetColor(0), Width = 100 });

		for (int i = 1; i < 16; i++)
			Items.Add(new Issue23704Model { Index = i, Color = GetColor(i), Width = 50 });

		Items.Add(new Issue23704Model { Index = 16, Color = GetColor(17), Width = 100 });
		Items.Add(new Issue23704Model { Index = 17, Color = GetColor(18), Width = 100 });

		Items2.Add(new Issue23704Model { Index = 0, Color = GetColor(0), Width = 50 });

		for (int i = 1; i < 16; i++)
			Items2.Add(new Issue23704Model { Index = i, Color = GetColor(i + 1), Width = 50 });

		Items2.Add(new Issue23704Model { Index = 16, Color = GetColor(17), Width = 100 });
		Items2.Add(new Issue23704Model { Index = 17, Color = GetColor(18), Width = 100 });
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

public class Issue23704Model
{
	public Color Color { get; set; }
	public int Width { get; set; }
	public int Index { get; set; }
}
