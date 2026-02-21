using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows.Input;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Graphics;
using Microsoft.Maui.Layouts;

namespace Maui.Controls.Sample;
public enum BindableItemsSourceType
{
	None,
	ObservableCollectionT,
	EmptyObservableCollectionT,
}
public class BindableLayoutViewModel : INotifyPropertyChanged
{
	private object _stackEmptyView;
	private object _flexEmptyView;
	private object _gridEmptyView;
	private DataTemplate _emptyViewTemplate;
	private DataTemplate _itemTemplate;
	private DataTemplateSelector _itemTemplateSelector;
	private BindableItemsSourceType _itemsSourceType = BindableItemsSourceType.None;
	private ObservableCollection<BindableLayoutTestItem> _observableCollection;
	private ObservableCollection<BindableLayoutTestItem> _emptyObservableCollection;


	private readonly string[] _addSequenceFruits =
	{
		"Dragonfruit", "Passionfruit", "Starfruit", "Rambutan", "Durian", "Persimmon"
	};
	private int _addIndex = 0;

	public ICommand AddItemCommand { get; }

	public event PropertyChangedEventHandler PropertyChanged;

	public BindableLayoutViewModel()
	{
		LoadItems();
		ItemTemplate = new DataTemplate(() =>
			{

				var label = new Label
				{
					HorizontalOptions = LayoutOptions.Center,
					VerticalOptions = LayoutOptions.Center,
					FontAttributes = FontAttributes.Bold
				};
				label.SetBinding(Label.TextProperty, "Caption");

				return label;
			});

		AddItemCommand = new Command(AddItem);
	}

	private bool _gridEventsSubscribed = false;
	public void OnGridLoaded(object sender, EventArgs e)
	{
		if (sender is not Grid grid)
			return;

		if (!_gridEventsSubscribed)
		{
			grid.ChildAdded += (_, _) => ArrangeGridItems(grid);
			grid.ChildRemoved += (_, _) => ArrangeGridItems(grid);
			_gridEventsSubscribed = true;
		}
		ArrangeGridItems(grid);
	}

	private void ArrangeGridItems(Grid grid)
	{
		const int columns = 2; // Change to 3 or more if you want more columns

		var children = grid.Children.OfType<View>().ToList();

		// Clear definitions
		grid.RowDefinitions.Clear();
		grid.ColumnDefinitions.Clear();

		for (int i = 0; i < columns; i++)
			grid.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Star });

		int totalRows = (int)Math.Ceiling((double)children.Count / columns);
		for (int i = 0; i < totalRows; i++)
			grid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });

		// Reposition all children
		for (int index = 0; index < children.Count; index++)
		{
			int row = index / columns;
			int column = index % columns;
			Microsoft.Maui.Controls.Grid.SetRow(children[index], row);
			Microsoft.Maui.Controls.Grid.SetColumn(children[index], column);
		}
	}

	public object StackEmptyView
	{
		get => _stackEmptyView;
		set { _stackEmptyView = value; OnPropertyChanged(); }
	}

	public object FlexEmptyView
	{
		get => _flexEmptyView;
		set { _flexEmptyView = value; OnPropertyChanged(); }
	}

	public object GridEmptyView
	{
		get => _gridEmptyView;
		set { _gridEmptyView = value; OnPropertyChanged(); }
	}

	public DataTemplate EmptyViewTemplate
	{
		get => _emptyViewTemplate;
		set { _emptyViewTemplate = value; OnPropertyChanged(); }
	}

	public DataTemplate ItemTemplate
	{
		get => _itemTemplate;
		set { _itemTemplate = value; OnPropertyChanged(); }
	}

	public DataTemplateSelector ItemTemplateSelector
	{
		get => _itemTemplateSelector;
		set
		{
			if (_itemTemplateSelector != value)
			{
				_itemTemplateSelector = value;
				if (_itemTemplateSelector != null && _itemTemplate != null)
				{
					_itemTemplate = null;
					OnPropertyChanged(nameof(ItemTemplate));
				}
				OnPropertyChanged();
			}
		}
	}

	public BindableItemsSourceType ItemsSourceType
	{
		get => _itemsSourceType;
		set
		{
			if (_itemsSourceType != value)
			{
				_itemsSourceType = value;
				OnPropertyChanged();
				OnPropertyChanged(nameof(ItemsSource));
			}
		}
	}

	public object ItemsSource
	{
		get
		{
			return ItemsSourceType switch
			{
				BindableItemsSourceType.ObservableCollectionT => _observableCollection,
				BindableItemsSourceType.EmptyObservableCollectionT => _emptyObservableCollection,
				BindableItemsSourceType.None => null,
				_ => null
			};
		}
	}

	private void LoadItems()
	{
		_observableCollection = new ObservableCollection<BindableLayoutTestItem>();
		AddItems(_observableCollection, 2, "Fruits");
		AddItems(_observableCollection, 2, "Vegetables");

		_emptyObservableCollection = new ObservableCollection<BindableLayoutTestItem>();
	}

	private void AddItem()
	{
		var newItem = new BindableLayoutTestItem($"NewItem {_addIndex}", _addIndex);
		_addIndex++;

		if (ItemsSourceType == BindableItemsSourceType.ObservableCollectionT && _observableCollection != null)
		{
			_observableCollection.Insert(0, newItem);
		}

		OnPropertyChanged(nameof(ItemsSource));
	}

	public void AddSequentialItem()
	{
		if (_addIndex >= _addSequenceFruits.Length)
			_addIndex = 0;

		var fruitName = _addSequenceFruits[_addIndex++];
		var newItem = new BindableLayoutTestItem(fruitName, _addIndex - 1);

		if (ItemsSourceType == BindableItemsSourceType.ObservableCollectionT && _observableCollection != null)
		{
			_observableCollection.Insert(0, newItem);
		}

		OnPropertyChanged(nameof(ItemsSource));
	}

	public void RemoveLastItem()
	{
		object deletedItem = null;
		if (ItemsSourceType == BindableItemsSourceType.ObservableCollectionT && _observableCollection != null && _observableCollection.Count > 0)
		{
			deletedItem = _observableCollection[^1];
			_observableCollection.RemoveAt(_observableCollection.Count - 1);
		}

		OnPropertyChanged(nameof(ItemsSource));
	}

	public void AddItemAtIndex(int index)
	{
		string[] fruits = { "Kiwi", "Guava", "Chikoo", "Raseberry", "Papaya", "Pineapple", "Strawberry", "Blueberry", "Peach", "Cherry" };
		string sequentialItem = fruits[index % fruits.Length];

		switch (ItemsSourceType)
		{
			case BindableItemsSourceType.ObservableCollectionT:
				if (index >= 0 && _observableCollection != null && index <= _observableCollection.Count)
				{
					_observableCollection.Insert(index, new BindableLayoutTestItem(sequentialItem, index));
				}
				break;
		}

		OnPropertyChanged(nameof(ItemsSource));
	}

	public void RemoveItemAtIndex(int index)
	{
		object deletedItem = null;
		if (ItemsSourceType == BindableItemsSourceType.ObservableCollectionT && _observableCollection != null)
		{
			if (index >= 0 && index < _observableCollection.Count)
			{
				deletedItem = _observableCollection[index];
				_observableCollection.RemoveAt(index);
			}
		}

		OnPropertyChanged(nameof(ItemsSource));
	}
	private readonly string[] _replaceSequenceAnimals = { "Cat", "Dog", "Monkey", "Deer", "Bear", "Lion", "Tiger", "Elephant", "Fox", "Zebra" };
	private int _replaceIndex = 0;
	public void ReplaceItem()
	{
		if (ItemsSourceType != BindableItemsSourceType.ObservableCollectionT ||
			_observableCollection == null || _observableCollection.Count == 0)
			return;

		if (_replaceIndex >= _observableCollection.Count)
			_replaceIndex = 0;

		var newCaption = _replaceSequenceAnimals[_replaceIndex % _replaceSequenceAnimals.Length];
		_observableCollection[_replaceIndex] = new BindableLayoutTestItem(newCaption, _replaceIndex);

		_replaceIndex++;
		OnPropertyChanged(nameof(ItemsSource));
	}

	public void ReplaceItemAtIndex(int index)
	{
		if (ItemsSourceType != BindableItemsSourceType.ObservableCollectionT ||
			_observableCollection == null || index < 0 || index >= _observableCollection.Count)
			return;

		var newItem = new BindableLayoutTestItem(
			_replaceSequenceAnimals[index % _replaceSequenceAnimals.Length], index);

		_observableCollection[index] = newItem;
		OnPropertyChanged(nameof(ItemsSource));
	}


	private void AddItems(IList<BindableLayoutTestItem> list, int count, string category)
	{
		string[] fruits =
		{
			"Apple", "Banana", "Orange", "Grapes", "Mango",
			"Pineapple", "Strawberry", "Blueberry", "Peach", "Cherry",
			"Watermelon", "Papaya", "Kiwi", "Pear", "Plum",
			"Avocado", "Fig", "Guava", "Lychee", "Pomegranate",
			"Lime", "Lemon", "Coconut", "Apricot", "Blackberry"
		};

		string[] vegetables =
		{
			"Carrot", "Broccoli", "Spinach", "Potato", "Tomato",
			"Cucumber", "Lettuce", "Onion", "Garlic", "Pepper",
			"Zucchini", "Pumpkin", "Radish", "Beetroot", "Cabbage",
			"Sweet Potato", "Turnip", "Cauliflower", "Celery", "Asparagus",
			"Eggplant", "Chili", "Corn", "Peas", "Mushroom"
		};

		string[] items = category == "Fruits" ? fruits : vegetables;

		for (int n = 0; n < count; n++)
		{
			list.Add(new BindableLayoutTestItem(items[n % items.Length], n));
		}
	}

	protected void OnPropertyChanged([CallerMemberName] string propertyName = null)
	{
		PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
	}

	public class CustomDataTemplateSelector : DataTemplateSelector
	{
		public DataTemplate Template1 { get; set; }
		public DataTemplate Template2 { get; set; }

		protected override DataTemplate OnSelectTemplate(object item, BindableObject container)
		{
			if (item is BindableLayoutTestItem testItem)
			{
				return testItem.Index % 2 == 0 ? Template1 : Template2;
			}

			return Template1;
		}
	}
}
public class BindableLayoutTestItem
{
	public string Caption { get; set; }
	public int Index { get; set; }

	public BindableLayoutTestItem(string caption, int index)
	{
		Caption = caption;
		Index = index;
	}
}