using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Graphics;
using System.Collections.Specialized;

namespace Maui.Controls.Sample;

public class GroupingItemsSource<TKey, TItem> : ObservableCollection<TItem>
{
	public TKey Key { get; }

	public GroupingItemsSource(TKey key, IEnumerable<TItem> items) : base(items)
	{
		Key = key;
	}
}

public enum ItemsSourceType1
{
	None,
	ListT,
	ListModelT,
	ObservableCollectionT,
	ObservableCollectionModelT,
	GroupedListT,
	GroupedListModelT,
	EmptyGroupedListT,
	EmptyGroupedListModelT,
	EmptyObservableCollectionT,
	EmptyObservableCollectionModelT
}

public class ItemsSourceViewModel : INotifyPropertyChanged
{
	private DataTemplate _groupHeaderTemplate;
	private DataTemplate _itemTemplate;
	private IItemsLayout _itemsLayout = new LinearItemsLayout(ItemsLayoutOrientation.Vertical);
	private ItemsSourceType1 _itemsSourceType = ItemsSourceType1.None;
	private bool _isGrouped = false;
	private bool _itemsSourceModelItems = false;
	private bool _itemsSourceStringItems = true;

	private ObservableCollection<CollectionViewTestItem> _observableCollection;
	private ObservableCollection<CollectionViewTestModelItem> _observableCollectionModel;
	private ObservableCollection<CollectionViewTestItem> _emptyObservableCollection;
	private ObservableCollection<CollectionViewTestModelItem> _emptyObservableCollectionModel;

	private List<GroupingItemsSource<string, CollectionViewTestItem>> _groupedList;
	private List<GroupingItemsSource<string, CollectionViewTestModelItem>> _groupedListModel;
	private List<GroupingItemsSource<string, CollectionViewTestItem>> _emptyGroupedList;
	private List<GroupingItemsSource<string, CollectionViewTestModelItem>> _emptyGroupedListModel;

	private List<CollectionViewTestItem> _list;
	private List<CollectionViewTestModelItem> _listModel;
	private SelectionMode _selectionMode = SelectionMode.None;
	private object _selectedItem;
	private ObservableCollection<object> _selectedItems = new ObservableCollection<object>();
	private string _previousSelectionText;
	private string _currentSelectionText;
	public event PropertyChangedEventHandler PropertyChanged;

	public ItemsSourceViewModel()
	{
		LoadItems();
		UpdateItemTemplate();
		GroupHeaderTemplate = new DataTemplate(() =>
		{
			var stackLayout = new StackLayout
			{
				BackgroundColor = Colors.LightGray
			};
			var label = new Label
			{
				FontAttributes = FontAttributes.Bold,
				FontSize = 18
			};
			label.SetBinding(Label.TextProperty, "Key");
			stackLayout.Children.Add(label);
			return stackLayout;
		});

		SelectedItems = new ObservableCollection<object>();
	}

	public DataTemplate GroupHeaderTemplate
	{
		get => _groupHeaderTemplate;
		set
		{
			_groupHeaderTemplate = value;
			OnPropertyChanged();
		}
	}

	public DataTemplate ItemTemplate
	{
		get => _itemTemplate;
		set
		{
			_itemTemplate = value;
			OnPropertyChanged();
		}
	}

	public IItemsLayout ItemsLayout
	{
		get => _itemsLayout;
		set
		{
			if (_itemsLayout != value)
			{
				_itemsLayout = value;
				OnPropertyChanged();
			}
		}
	}

	public ItemsSourceType1 ItemsSourceType1
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

	public bool IsGrouped
	{
		get => _isGrouped;
		set
		{
			if (_isGrouped != value)
			{
				_isGrouped = value;
				OnPropertyChanged();
				OnPropertyChanged(nameof(ItemsSource));
			}
		}
	}

	public bool ItemsSourceModelItems
	{
		get => _itemsSourceModelItems;
		set
		{
			if (_itemsSourceModelItems != value)
			{
				_itemsSourceModelItems = value;
				OnPropertyChanged();
				UpdateItemTemplate();
			}
		}
	}

	public bool ItemsSourceStringItems
	{
		get => _itemsSourceStringItems;
		set
		{
			if (_itemsSourceStringItems != value)
			{
				_itemsSourceStringItems = value;
				OnPropertyChanged();
				UpdateItemTemplate();
			}
		}
	}
	public object ItemsSource
	{
		get
		{
			return ItemsSourceType1 switch
			{
				ItemsSourceType1.ObservableCollectionT => _observableCollection,
				ItemsSourceType1.ObservableCollectionModelT => _observableCollectionModel,
				ItemsSourceType1.GroupedListT => _groupedList,
				ItemsSourceType1.GroupedListModelT => _groupedListModel,
				ItemsSourceType1.ListT => _list,
				ItemsSourceType1.ListModelT => _listModel,
				ItemsSourceType1.EmptyGroupedListT => _emptyGroupedList,
				ItemsSourceType1.EmptyGroupedListModelT => _emptyGroupedListModel,
				ItemsSourceType1.EmptyObservableCollectionT => _emptyObservableCollection,
				ItemsSourceType1.EmptyObservableCollectionModelT => _emptyObservableCollectionModel,
				_ => null
			};
		}
	}

	public SelectionMode SelectionMode
	{
		get => _selectionMode;
		set
		{
			if (_selectionMode != value)
			{
				_selectionMode = value;
				OnPropertyChanged();
			}
		}
	}
	public object SelectedItem
	{
		get => _selectedItem;
		set
		{
			if (_selectedItem != value)
			{
				_selectedItem = value;
				OnPropertyChanged();
			}
		}
	}

	public ObservableCollection<object> SelectedItems
	{
		get => _selectedItems;
		set
		{
			if (_selectedItems != value)
			{
				_selectedItems = value;
				OnPropertyChanged();
			}
		}
	}

	public string PreviousSelectionText
	{
		get => _previousSelectionText;
		set { _previousSelectionText = value; OnPropertyChanged(); }
	}

	public string CurrentSelectionText
	{
		get => _currentSelectionText;
		set { _currentSelectionText = value; OnPropertyChanged(); }
	}
	private void LoadItems()
	{
		_observableCollection = new ObservableCollection<CollectionViewTestItem>();
		AddItems(_observableCollection, 2, "Fruits");
		AddItems(_observableCollection, 2, "Vegetables");

		_observableCollectionModel = new ObservableCollection<CollectionViewTestModelItem>();
		AddItems(_observableCollectionModel, 2, "Group A");
		AddItems(_observableCollectionModel, 1, "Group B");

		_list = new List<CollectionViewTestItem>();
		AddItems(_list, 2, "Fruits");
		AddItems(_list, 2, "Vegetables");

		_listModel = new List<CollectionViewTestModelItem>();
		AddItems(_listModel, 2, "Group A");

		_emptyObservableCollection = new ObservableCollection<CollectionViewTestItem>();
		_emptyObservableCollectionModel = new ObservableCollection<CollectionViewTestModelItem>();

		_groupedList = new List<GroupingItemsSource<string, CollectionViewTestItem>>
		{
			new GroupingItemsSource<string, CollectionViewTestItem>("Fruits", new List<CollectionViewTestItem>()),
			new GroupingItemsSource<string, CollectionViewTestItem>("Vegetables", new List<CollectionViewTestItem>())
		};
		AddItems(_groupedList[0], 3, "Fruits");
		AddItems(_groupedList[1], 3, "Vegetables");

		_groupedListModel = new List<GroupingItemsSource<string, CollectionViewTestModelItem>>
		{
			new GroupingItemsSource<string, CollectionViewTestModelItem>("Group A", new List<CollectionViewTestModelItem>()),
			new GroupingItemsSource<string, CollectionViewTestModelItem>("Group B", new List<CollectionViewTestModelItem>())
		};
		AddItems(_groupedListModel[0], 1, "Group A");
		AddItems(_groupedListModel[1], 1, "Group B");

		_emptyGroupedList = new List<GroupingItemsSource<string, CollectionViewTestItem>>();
		_emptyGroupedListModel = new List<GroupingItemsSource<string, CollectionViewTestModelItem>>();
	}

	private void AddItems(IList<CollectionViewTestItem> list, int count, string category)
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
			list.Add(new CollectionViewTestItem(items[n % items.Length], n)); // Pass the index
		}
	}

	private void AddItems(IList<CollectionViewTestModelItem> list, int count, string category)
	{
		string[] groupA =
		{
				"dotnet_bot.png",
				"calculator.png",
				"blue.png",
				"bank.png",
			};
		string[] groupB =
		{
				"avatar.png",
				"coffee.png",
				"cover1.jpg",
				"menu_icon.png",
			};
		string[] imageItems = category == "Group A" ? groupA : groupB;

		for (int n = 0; n < count; n++)
		{
			list.Add(new CollectionViewTestModelItem(
				$"{imageItems[n % imageItems.Length]}", imageItems[n % imageItems.Length], n));
		}
	}

	private void UpdateItemTemplate()
	{
		if (ItemsSourceStringItems && !ItemsSourceModelItems)
		{
			ItemTemplate = new DataTemplate(() =>
			{
				var stackLayout = new StackLayout
				{
					Padding = new Thickness(10),
					HorizontalOptions = LayoutOptions.Center,
					VerticalOptions = LayoutOptions.Center
				};

				var label = new Label
				{
					VerticalOptions = LayoutOptions.Center,
					HorizontalOptions = LayoutOptions.Center
				};
				label.SetBinding(Label.TextProperty, "Caption");
				stackLayout.Children.Add(label);
				return stackLayout;
			});
		}
		else if (ItemsSourceModelItems && !ItemsSourceStringItems)
		{
			ItemTemplate = ExampleTemplates.PhotoTemplate();
		}
	}
	private int fruitIndex = 0;
	private int groupAIndex = 0;
	public void AddSequentialItem()
	{
		string[] fruits = { "Kiwi", "Guava", "Chikoo", "Raseberry", "Papaya", "Pineapple", "Strawberry", "Blueberry", "Peach", "Cherry" };
		string[] groupA = { "green.png", "groceries.png", "oasis.jpg", "seth.png", "test.jpg", "vegetables.jpg" };

		string nextItem = ItemsSourceType1 switch
		{
			ItemsSourceType1.ObservableCollectionT or ItemsSourceType1.GroupedListT => fruits[fruitIndex++ % fruits.Length],
			ItemsSourceType1.ObservableCollectionModelT or ItemsSourceType1.GroupedListModelT => groupA[groupAIndex++ % groupA.Length],
			_ => throw new InvalidOperationException("Unsupported ItemsSourceType for adding items.")
		};

		switch (ItemsSourceType1)
		{
			case ItemsSourceType1.ObservableCollectionT:
				_observableCollection.Add(new CollectionViewTestItem(nextItem, _observableCollection.Count));
				break;

			case ItemsSourceType1.ObservableCollectionModelT:
				_observableCollectionModel.Add(new CollectionViewTestModelItem(nextItem, nextItem, _observableCollectionModel.Count));
				break;

			case ItemsSourceType1.GroupedListT:
				if (_groupedList.Count > 1)
				{
					int targetGroupIndex = _groupedList[0].Count <= _groupedList[1].Count ? 0 : 1;
					_groupedList[targetGroupIndex].Add(new CollectionViewTestItem(nextItem, _groupedList[targetGroupIndex].Count));
				}
				else if (_groupedList.Count > 0)
				{
					_groupedList[0].Add(new CollectionViewTestItem(nextItem, _groupedList[0].Count));
				}
				break;

			case ItemsSourceType1.GroupedListModelT:
				if (_groupedListModel.Count > 1)
				{
					int targetGroupIndex = _groupedListModel[0].Count <= _groupedListModel[1].Count ? 0 : 1;
					_groupedListModel[targetGroupIndex].Add(new CollectionViewTestModelItem(nextItem, nextItem, _groupedListModel[targetGroupIndex].Count));
				}
				else if (_groupedListModel.Count > 0)
				{
					_groupedListModel[0].Add(new CollectionViewTestModelItem(nextItem, nextItem, _groupedListModel[0].Count));
				}
				break;

			default:
				throw new InvalidOperationException("Unsupported ItemsSourceType for adding items.");
		}

		OnPropertyChanged(nameof(ItemsSource));
	}

	public void RemoveLastItem()
	{
		object deletedItem = null;
		switch (ItemsSourceType1)
		{
			case ItemsSourceType1.ObservableCollectionT:
				if (_observableCollection.Count > 0)
				{
					deletedItem = _observableCollection[^1];
					_observableCollection.RemoveAt(_observableCollection.Count - 1);
				}
				break;

			case ItemsSourceType1.ObservableCollectionModelT:
				if (_observableCollectionModel.Count > 0)
				{
					deletedItem = _observableCollectionModel[^1];
					_observableCollectionModel.RemoveAt(_observableCollectionModel.Count - 1);
				}
				break;

			case ItemsSourceType1.GroupedListT:
				if (_groupedList.Count > 0)
				{
					int targetGroupIndex = (_groupedList[0].Count > 0) ? 0 : 1;

					if (targetGroupIndex < _groupedList.Count && _groupedList[targetGroupIndex].Count > 0)
					{
						deletedItem = _groupedList[targetGroupIndex][^1];
						_groupedList[targetGroupIndex].RemoveAt(_groupedList[targetGroupIndex].Count - 1);
					}
				}
				break;

			case ItemsSourceType1.GroupedListModelT:
				if (_groupedListModel.Count > 0)
				{
					int targetGroupIndex = (_groupedListModel[0].Count > 0) ? 0 : 1;

					if (targetGroupIndex < _groupedListModel.Count && _groupedListModel[targetGroupIndex].Count > 0)
					{
						deletedItem = _groupedListModel[targetGroupIndex][^1];
						_groupedListModel[targetGroupIndex].RemoveAt(_groupedListModel[targetGroupIndex].Count - 1);
					}
				}
				break;

			default:
				throw new InvalidOperationException("Unsupported ItemsSourceType for removing items.");
		}

		if (deletedItem != null)
		{
			if (SelectedItems.Contains(deletedItem))
			{
				SelectedItems.Remove(deletedItem);
			}

			if (SelectedItem == deletedItem)
			{
				SelectedItem = null;
			}
		}

		OnPropertyChanged(nameof(ItemsSource));
	}

	public void AddItemAtIndex(int index)
	{
		string[] fruits = { "Kiwi", "Guava", "Chikoo", "Raseberry", "Papaya", "Pineapple", "Strawberry", "Blueberry", "Peach", "Cherry" };
		string[] groupA = { "green.png", "groceries.png", "oasis.jpg", "seth.png", "test.jpg", "vegetables.jpg" };

		string sequentialItem = fruits[index % fruits.Length];
		string sequentialImageItem = groupA[index % groupA.Length];

		switch (ItemsSourceType1)
		{
			case ItemsSourceType1.ObservableCollectionT:
				if (index >= 0 && index <= _observableCollection.Count)
				{
					_observableCollection.Insert(index, new CollectionViewTestItem(sequentialItem, index));
				}
				break;

			case ItemsSourceType1.ObservableCollectionModelT:
				if (index >= 0 && index <= _observableCollectionModel.Count)
				{
					_observableCollectionModel.Insert(index, new CollectionViewTestModelItem(sequentialImageItem, sequentialImageItem, index));
				}
				break;

			case ItemsSourceType1.GroupedListT:
				if (_groupedList.Count > 0 && index >= 0 && index <= _groupedList[0].Count)
				{
					_groupedList[0].Insert(index, new CollectionViewTestItem(sequentialItem, index));
				}
				break;

			case ItemsSourceType1.GroupedListModelT:
				if (_groupedListModel.Count > 0 && index >= 0 && index <= _groupedListModel[0].Count)
				{
					_groupedListModel[0].Insert(index, new CollectionViewTestModelItem(sequentialImageItem, sequentialImageItem, index));
				}
				break;

			default:
				throw new InvalidOperationException("Unsupported ItemsSourceType for adding items.");
		}

		OnPropertyChanged(nameof(ItemsSource));
	}
	public void RemoveItemAtIndex(int index)
	{
		object deletedItem = null;

		switch (ItemsSourceType1)
		{
			case ItemsSourceType1.ObservableCollectionT:
				if (index >= 0 && index < _observableCollection.Count)
				{
					deletedItem = _observableCollection[index];
					_observableCollection.RemoveAt(index);
				}
				break;

			case ItemsSourceType1.ObservableCollectionModelT:
				if (index >= 0 && index < _observableCollectionModel.Count)
				{
					deletedItem = _observableCollectionModel[index];
					_observableCollectionModel.RemoveAt(index);
				}
				break;

			case ItemsSourceType1.GroupedListT:
				if (_groupedList.Count > 0)
				{
					// Remove from _groupedList[0] if the index is valid
					if (index >= 0 && index < _groupedList[0].Count)
					{
						deletedItem = _groupedList[0][index];
						_groupedList[0].RemoveAt(index);
					}
					// If _groupedList[0] is empty or index is invalid, remove from _groupedList[1]
					else if (_groupedList.Count > 1 && index >= 0 && index < _groupedList[1].Count)
					{
						deletedItem = _groupedList[1][index];
						_groupedList[1].RemoveAt(index);
					}
				}
				break;

			case ItemsSourceType1.GroupedListModelT:
				if (_groupedListModel.Count > 0)
				{
					// Remove from _groupedListModel[0] if the index is valid
					if (index >= 0 && index < _groupedListModel[0].Count)
					{
						deletedItem = _groupedListModel[0][index];
						_groupedListModel[0].RemoveAt(index);
					}
					// If _groupedListModel[0] is empty or index is invalid, remove from _groupedListModel[1]
					else if (_groupedListModel.Count > 1 && index >= 0 && index < _groupedListModel[1].Count)
					{
						deletedItem = _groupedListModel[1][index];
						_groupedListModel[1].RemoveAt(index);
					}
				}
				break;

			default:
				throw new InvalidOperationException("Unsupported ItemsSourceType for removing items.");
		}

		if (deletedItem != null)
		{
			if (SelectedItems.Contains(deletedItem))
			{
				SelectedItems.Remove(deletedItem);
			}

			if (SelectedItem == deletedItem)
			{
				SelectedItem = null;
			}
		}

		OnPropertyChanged(nameof(ItemsSource));
	}
	protected void OnPropertyChanged([CallerMemberName] string propertyName = null)
	{
		if (propertyName == nameof(IsGrouped))
		{
			OnPropertyChanged(nameof(ItemsSource));
		}

		PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
	}

	public class CollectionViewTestItem
	{
		public string Caption { get; set; }
		public int Index { get; set; }

		public CollectionViewTestItem(string caption, int index)
		{
			Caption = caption;
			Index = index;
		}
	}

	public class CollectionViewTestModelItem
	{
		public string Caption { get; set; }
		public string Image { get; set; }
		public int Index { get; set; }

		public CollectionViewTestModelItem(string caption, string image, int index)
		{
			Caption = caption;
			Image = image;
			Index = index;
		}
	}
	internal class ExampleTemplates
	{
		public static DataTemplate PhotoTemplate()
		{
			return new DataTemplate(() =>
			{
				var templateLayout = new Grid
				{
					RowDefinitions = new RowDefinitionCollection { new RowDefinition(), new RowDefinition() },
					WidthRequest = 200,
					HeightRequest = 100,
				};
				var image = new Image
				{
					WidthRequest = 100,
					HorizontalOptions = LayoutOptions.Center,
					VerticalOptions = LayoutOptions.Center,
					Margin = new Thickness(2, 5, 2, 2),
					AutomationId = "photo"
				};
				image.SetBinding(Image.SourceProperty, new Binding("Image"));
				var caption = new Label
				{
					FontSize = 12,
					HorizontalOptions = LayoutOptions.Fill,
					HorizontalTextAlignment = TextAlignment.Center,
					Margin = new Thickness(2, 0, 2, 2),
					BackgroundColor = Colors.Blue
				};
				caption.SetBinding(Label.TextProperty, new Binding("Caption"));
				templateLayout.Children.Add(image);
				templateLayout.Children.Add(caption);
				Grid.SetRow(caption, 1);
				return templateLayout;
			});
		}
	}
}