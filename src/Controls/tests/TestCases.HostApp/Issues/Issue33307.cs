using System.Collections.ObjectModel;

namespace Maui.Controls.Sample.Issues;

[Issue(IssueTracker.Github, 33307, "The Picker is still binding to the property and reacts to data changes after the page is closed.", PlatformAffected.UWP)]

public class Issue33307 : TestNavigationPage
{
	protected override void Init()
	{
		Navigation.PushAsync(new Issue33307ContentPage());
	}
}

public class Issue33307ContentPage : ContentPage
{
	Issue33307ClassA mainData;

	public Issue33307ContentPage()
	{
		mainData = new Issue33307ClassA();

		Title = "MainPage";

		var stackLayout = new HorizontalStackLayout
		{
			Spacing = 100,
			HorizontalOptions = LayoutOptions.Center,
			VerticalOptions = LayoutOptions.Center
		};

		var buttonPage1 = new Button
		{
			Text = "Page1",
			AutomationId = "Page1",
			HeightRequest = 180,
			WidthRequest = 180
		};
		buttonPage1.Clicked += ButtonPage1_Clicked;

		var buttonPage2 = new Button
		{
			Text = "Page2",
			AutomationId = "Page2",
			HeightRequest = 180,
			WidthRequest = 180
		};
		buttonPage2.Clicked += ButtonPage2_Clicked;

		stackLayout.Add(buttonPage1);
		stackLayout.Add(buttonPage2);

		Content = new Grid
		{
			Children = { stackLayout }
		};
	}

	async void ButtonPage1_Clicked(object? sender, EventArgs e)
	{
		await Navigation.PushAsync(new Issue33307Page1(mainData));
	}

	async void ButtonPage2_Clicked(object? sender, EventArgs e)
	{
		await Navigation.PushAsync(new Issue33307Page2(mainData));
	}
}

public class Issue33307ClassA
{
	public int id_counter { get; set; }
	public ObservableCollection<Issue33307ClassB> itemsB { get; set; } = new();
	public ObservableCollection<Issue33307ClassC> itemsC { get; set; } = new();
}

public class Issue33307ClassB
{
	public int id { get; set; }
	public string name { get; set; } = string.Empty;
	public bool isSelected { get; set; }

	public object Clone()
	{
		return MemberwiseClone();
	}
}

public class Issue33307ClassC : INotifyPropertyChanged
{
	public int id { get; set; }
	public string name { get; set; } = string.Empty;
	public ObservableCollection<Issue33307ClassB> itemsB { get; set; } = new();

	public event PropertyChangedEventHandler? PropertyChanged;

	public Issue33307ClassB? selected_item
	{
		get
		{
			for (var i = 0; i < itemsB.Count; i++)
			{
				if (itemsB[i].isSelected)
					return itemsB[i];
			}
			return null;
		}
		set
		{
			if (value == null)
			{
				for (var i = 0; i < itemsB.Count; i++)
					itemsB[i].isSelected = false;
			}
			else
			{
				for (var i = 0; i < itemsB.Count; i++)
				{
					if (itemsB[i].name == value.name)
					{
						itemsB[i].isSelected = true;
					}
					else
					{
						itemsB[i].isSelected = false;
					}
				}
			}
			PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(selected_item)));
		}
	}
}

public class Issue33307Page1 : ContentPage
{
	Issue33307ClassA mainData;
	CollectionView collectionView1;

	public Issue33307Page1(Issue33307ClassA mainData)
	{
		this.mainData = mainData;
		Title = "Page1";

		var headerGrid = new Grid
		{
			ColumnDefinitions =
			{
				new ColumnDefinition(GridLength.Star),
				new ColumnDefinition(new GridLength(60))
			},
			ColumnSpacing = 5
		};

		var nameLabel = new Label { Text = "Name", VerticalOptions = LayoutOptions.Center };
		Grid.SetColumn(nameLabel, 0);
		headerGrid.Add(nameLabel);

		var deleteLabel = new Label { Text = "Delete", VerticalOptions = LayoutOptions.Center };
		Grid.SetColumn(deleteLabel, 1);
		headerGrid.Add(deleteLabel);

		var headerLayout = new VerticalStackLayout
		{
			Spacing = 10,
			VerticalOptions = LayoutOptions.End,
			Children =
			{
				headerGrid,
				new BoxView { Color = Colors.Black, HeightRequest = 2, CornerRadius = 20 }
			}
		};

		collectionView1 = new CollectionView
		{
			HorizontalOptions = LayoutOptions.Fill,
			BackgroundColor = Colors.WhiteSmoke,
			ItemTemplate = new DataTemplate(() =>
			{
				var itemGrid = new Grid
				{
					ColumnSpacing = 5,
					Margin = new Thickness(0, 8, 0, 8),
					ColumnDefinitions =
					{
						new ColumnDefinition(GridLength.Star),
						new ColumnDefinition(new GridLength(60))
					}
				};

				var entry = new Entry
				{
					Placeholder = "Name",
					VerticalOptions = LayoutOptions.Center
				};
				entry.SetBinding(Entry.TextProperty, "name");
				Grid.SetColumn(entry, 0);
				itemGrid.Add(entry);

				return new VerticalStackLayout
				{
					Margin = new Thickness(1, 0, 1, 0),
					Children =
					{
						itemGrid,
						new BoxView { Color = Colors.Black, HeightRequest = 2, CornerRadius = 20 }
					}
				};
			}),
			EmptyView = new Label
			{
				Text = "No items to display",
				HorizontalTextAlignment = TextAlignment.Center,
				VerticalTextAlignment = TextAlignment.Center
			}
		};
		collectionView1.ItemsSource = this.mainData.itemsB;

		var addButton = new Button
		{
			Text = "Add items",
			AutomationId = "AddItems",
			HorizontalOptions = LayoutOptions.Start
		};
		addButton.Clicked += ButtonAddRow_Clicked;

		var delButton = new Button
		{
			Text = "Delete",
			AutomationId = "Delete"
		};
		delButton.Clicked += ButtonDeleteRow_Clicked;

		var hStack = new HorizontalStackLayout
		{
			Spacing = 20,
			Children =
			{
				addButton,
				delButton
			}
		};

		var mainGrid = new Grid
		{
			RowDefinitions =
			{
				new RowDefinition(GridLength.Auto),
				new RowDefinition(GridLength.Star),
				new RowDefinition(new GridLength(50))
			},
			RowSpacing = 11,
			Margin = new Thickness(10, 10, 10, 20)
		};

		Grid.SetRow(headerLayout, 0);
		mainGrid.Add(headerLayout);

		Grid.SetRow(collectionView1, 1);
		mainGrid.Add(collectionView1);

		Grid.SetRow(hStack, 2);
		mainGrid.Add(hStack);

		Content = mainGrid;
	}

	void ButtonAddRow_Clicked(object? sender, EventArgs e)
	{
		var itemB1 = new Issue33307ClassB
		{
			id = mainData.id_counter++,
			name = "itemB1"
		};
		mainData.itemsB.Add(itemB1);

		var itemB2 = new Issue33307ClassB
		{
			id = mainData.id_counter++,
			name = "itemB2"
		};
		mainData.itemsB.Add(itemB2);

		var itemB3 = new Issue33307ClassB
		{
			id = mainData.id_counter++,
			name = "itemB3"
		};
		mainData.itemsB.Add(itemB3);
	}

	async void ButtonDeleteRow_Clicked(object? sender, EventArgs e)
	{
		if (mainData.itemsB.Count > 1)
		{
			var item = mainData.itemsB[1];
			RemoveInDependencies(item.id);
			mainData.itemsB.Remove(item);
		}
	}

	void RemoveInDependencies(int id)
	{
		foreach (var itemC in mainData.itemsC)
		{
			for (int i = 0; i < itemC.itemsB.Count; i++)
			{
				if (itemC.itemsB[i].id == id)
				{
					itemC.itemsB.RemoveAt(i);
					break;
				}
			}
		}
	}
}

public class Issue33307Page2 : ContentPage
{
	Issue33307ClassA mainData;
	CollectionView collectionView1;
	Label selectedItemLabel;

	public Issue33307Page2(Issue33307ClassA mainData)
	{
		this.mainData = mainData;
		Title = "Page2";

		var headerGrid = new Grid
		{
			ColumnDefinitions =
			{
				new ColumnDefinition(GridLength.Star),
				new ColumnDefinition(GridLength.Star),
				new ColumnDefinition(new GridLength(60))
			},
			ColumnSpacing = 5
		};

		var nameLabel = new Label
		{
			Text = "Name",
			VerticalOptions = LayoutOptions.Center,
			HorizontalOptions = LayoutOptions.Center
		};
		Grid.SetColumn(nameLabel, 0);
		headerGrid.Add(nameLabel);

		var itemBLabel = new Label
		{
			Text = "ItemB",
			VerticalOptions = LayoutOptions.Center,
			HorizontalOptions = LayoutOptions.Center
		};
		Grid.SetColumn(itemBLabel, 1);
		headerGrid.Add(itemBLabel);

		var deleteLabel = new Label
		{
			Text = "Delete",
			VerticalOptions = LayoutOptions.Center,
			HorizontalOptions = LayoutOptions.Center
		};
		Grid.SetColumn(deleteLabel, 2);
		headerGrid.Add(deleteLabel);

		var headerLayout = new VerticalStackLayout
		{
			Spacing = 10,
			VerticalOptions = LayoutOptions.End,
			Children =
			{
				headerGrid,
				new BoxView { Color = Colors.Black, HeightRequest = 2, CornerRadius = 20 }
			}
		};

		collectionView1 = new CollectionView
		{
			HorizontalOptions = LayoutOptions.Fill,
			BackgroundColor = Colors.WhiteSmoke,
			ItemTemplate = new DataTemplate(() =>
			{
				var itemGrid = new Grid
				{
					ColumnSpacing = 5,
					ColumnDefinitions =
					{
						new ColumnDefinition(GridLength.Star),
						new ColumnDefinition(GridLength.Star),
						new ColumnDefinition(new GridLength(60))
					}
				};

				var entry = new Entry
				{
					Placeholder = "Name",
					VerticalOptions = LayoutOptions.Center
				};
				entry.SetBinding(Entry.TextProperty, "name");
				Grid.SetColumn(entry, 0);
				itemGrid.Add(entry);

				var rowPicker = new Picker
				{
					TextColor = Colors.Black,
					TitleColor = Colors.Gray
				};
				rowPicker.SetBinding(Picker.ItemsSourceProperty, "itemsB");
				rowPicker.ItemDisplayBinding = new Binding("name");
				rowPicker.SetBinding(Picker.SelectedItemProperty, "selected_item");
				rowPicker.SelectedIndexChanged += (s, e) =>
				{
					if (rowPicker.SelectedItem is ClassB selectedItem)
					{
						if (selectedItemLabel != null)
							selectedItemLabel.Text = selectedItem.name.ToString();
					}
					else
					{
						selectedItemLabel?.Text = "None";
					}
				};
				Grid.SetColumn(rowPicker, 1);
				itemGrid.Add(rowPicker);

				return new VerticalStackLayout
				{
					Margin = new Thickness(1, 0, 1, 0),
					Children =
					{
						itemGrid,
						new BoxView { Color = Colors.Black, HeightRequest = 2, CornerRadius = 20 }
					}
				};
			}),
			EmptyView = new Label
			{
				Text = "No items to display",
				HorizontalTextAlignment = TextAlignment.Center,
				VerticalTextAlignment = TextAlignment.Center
			}
		};
		collectionView1.ItemsSource = this.mainData.itemsC;

		var addButton = new Button
		{
			Text = "Add",
			AutomationId = "Add",
			HorizontalOptions = LayoutOptions.Start
		};
		addButton.Clicked += ButtonAddRow_Clicked;

		var delItem = new Button
		{
			Text = "Select Second Item",
			AutomationId = "SelectSecondItem",
			HorizontalOptions = LayoutOptions.Start
		};
		delItem.Clicked += ButtonDeleteRow_Clicked;

		selectedItemLabel = new Label
		{
			Text = "Last Selected: None",
			VerticalOptions = LayoutOptions.Center,
			TextColor = Colors.DarkBlue,
			FontAttributes = FontAttributes.Bold
		};

		var hStack = new HorizontalStackLayout
		{
			Spacing = 20,
			Children =
			{
				addButton,
				delItem,
				selectedItemLabel
			}
		};

		var mainGrid = new Grid
		{
			RowDefinitions =
			{
				new RowDefinition(GridLength.Auto),
				new RowDefinition(GridLength.Star),
				new RowDefinition(new GridLength(50))
			},
			RowSpacing = 11,
			Margin = new Thickness(10, 10, 10, 20)
		};

		Grid.SetRow(headerLayout, 0);
		mainGrid.Add(headerLayout);

		Grid.SetRow(collectionView1, 1);
		mainGrid.Add(collectionView1);

		Grid.SetRow(hStack, 2);
		mainGrid.Add(hStack);

		Content = mainGrid;
	}

	async void ButtonDeleteRow_Clicked(object? sender, EventArgs e)
	{
		foreach (var itemC in mainData.itemsC)
		{
			if (itemC.itemsB.Count > 1)
			{
				itemC.selected_item = itemC.itemsB[1];
			}
			else if (itemC.itemsB.Count > 0)
			{
				itemC.selected_item = itemC.itemsB[0];
			}
		}
	}

	void ButtonAddRow_Clicked(object? sender, EventArgs e)
	{
		var itemC = new Issue33307ClassC
		{
			id = mainData.id_counter++
		};

		foreach (var itemB in mainData.itemsB)
		{
			itemC.itemsB.Add((Issue33307ClassB)itemB.Clone());
		}

		mainData.itemsC.Add(itemC);
	}
}
