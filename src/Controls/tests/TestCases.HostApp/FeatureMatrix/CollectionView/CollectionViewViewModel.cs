using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Windows.Input;
using Maui.Controls.Sample.CollectionViewGalleries;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Graphics;

namespace Maui.Controls.Sample;

public class Grouping<TKey, TItem> : ObservableCollection<TItem>
{
	public TKey Key { get; }

	public Grouping(TKey key, IEnumerable<TItem> items) : base(items)
	{
		Key = key;
	}
}

public enum ItemsSourceType
{
	None,
	ObservableCollectionT,
	ObservableCollectionT2,
	ObservableCollectionT3,
	ObservableCollection25T,
	ObservableCollection5T,
	GroupedListT,
	EmptyGroupedListT,
	EmptyObservableCollectionT,
	GroupedListT2,
	GroupedListT3,
	ObservableCollectionStringT,
	ObservableCollectionModelT,
	ListT,
	ListModelT,
	GroupedListStringT,
	GroupedListModelT,
	EmptyGroupedListModelT,
	EmptyObservableCollectionModelT
}

public class CollectionViewViewModel : INotifyPropertyChanged
{
	private object _emptyView;
	private object _header;
	private object _footer;
	private DataTemplate _emptyViewTemplate;
	private DataTemplate _headerTemplate;
	private DataTemplate _footerTemplate;
	private DataTemplate _groupHeaderTemplate;
	private DataTemplate _groupFooterTemplate;
	private DataTemplate _itemTemplate;
	private IItemsLayout _itemsLayout = new LinearItemsLayout(ItemsLayoutOrientation.Vertical);
	private ItemsSourceType _itemsSourceType = ItemsSourceType.None;
	private bool _isGrouped = false;
	private bool _canReorderItems = false;
	private bool _canMixGroups = false;
	private bool _itemsSourceStringItems = true;
	private ItemSizingStrategy _itemSizingStrategy;
	private ItemsUpdatingScrollMode _itemsUpdatingScrollMode;
	private ObservableCollection<CollectionViewTestItem> _observableCollection;
	private ObservableCollection<CollectionViewTestItem> _observableCollection25;
	private ObservableCollection<CollectionViewTestItem> _observableCollection5;
	private ObservableCollection<CollectionViewTestItem> _emptyObservableCollection;
	private List<Grouping<string, CollectionViewTestItem>> _groupedList;
	private List<Grouping<string, CollectionViewTestItem>> _emptyGroupedList;
	private ObservableCollection<CollectionViewTestItem> _observableCollection3;
	private ObservableCollection<ItemModel> _observableCollection2;
	private List<Grouping<string, CollectionViewTestItem>> _groupedList3;
	private List<Grouping<string, ItemModel>> _groupedList2;
	private ObservableCollection<CollectionViewTestItem> _observableCollectionString;
	private ObservableCollection<CollectionViewTestModelItem> _observableCollectionModel;
	private ObservableCollection<CollectionViewTestModelItem> _emptyObservableCollectionModel;
	private List<Grouping<string, CollectionViewTestItem>> _groupedListString;
	private List<Grouping<string, CollectionViewTestModelItem>> _groupedListModel;
	private List<Grouping<string, CollectionViewTestModelItem>> _emptyGroupedListModel;
	private List<CollectionViewTestItem> _list;
	private List<CollectionViewTestModelItem> _listModel;
	private SelectionMode _selectionMode = SelectionMode.None;
	private object _selectedItem;
	private ObservableCollection<object> _selectedItems = new ObservableCollection<object>();
	private int _selectionChangedEventCount = 0;
	private string _previousSelectionText;
	private string _currentSelectionText;
	private int fruitIndex = 0;
	private int groupAIndex = 0;
	private bool _isHeaderStringSelected;
	private bool _isFooterStringSelected;
	private bool _isHeaderGridSelected;
	private bool _isFooterGridSelected;
	private bool _isHeaderTemplateViewSelected;
	private bool _isFooterTemplateViewSelected;
	private bool _isGroupHeaderTemplateViewSelected;
	private bool _isGroupFooterTemplateViewSelected;
	private bool _isEmptyViewStringSelected;
	private bool _isEmptyViewGridSelected;
	private bool _isEmptyViewTemplateSelected;
	private bool _isItemTemplateSelected;

	public bool ShowAddRemoveButtons => ItemsSourceType == ItemsSourceType.ObservableCollectionT3 || ItemsSourceType == ItemsSourceType.GroupedListT3;


	public event PropertyChangedEventHandler PropertyChanged;

	private readonly string[] _addSequenceFruits =
	{
		"Dragonfruit", "Passionfruit", "Starfruit", "Rambutan", "Durian", "Persimmon"
	};
	private int _addIndex = 0;

	public ICommand AddItemCommand { get; }

	public CollectionViewViewModel()
	{
		LoadItems();

		AddItemCommand = new Command(AddItem);

		GroupHeaderTemplate = new DataTemplate(() =>
		{
			var stackLayout = new StackLayout
			{
				BackgroundColor = Colors.LightGray
			};
			var label = new Label
			{
				FontAttributes = FontAttributes.Bold,
				FontSize = 24,
			};
			label.SetBinding(Label.TextProperty, "Key");
			stackLayout.Children.Add(label);
			return stackLayout;
		});

		SetItemTemplate();
		SelectedItems = new ObservableCollection<object>();
		SelectedItems.CollectionChanged += OnSelectedItemsChanged;
	}

	public object EmptyView
	{
		get => _emptyView;
		set { _emptyView = value; OnPropertyChanged(); }
	}

	public object Header
	{
		get => _header;
		set { _header = value; OnPropertyChanged(); }
	}

	public object Footer
	{
		get => _footer;
		set { _footer = value; OnPropertyChanged(); }
	}

	public DataTemplate EmptyViewTemplate
	{
		get => _emptyViewTemplate;
		set { _emptyViewTemplate = value; OnPropertyChanged(); }
	}

	public DataTemplate HeaderTemplate
	{
		get => _headerTemplate;
		set { _headerTemplate = value; OnPropertyChanged(); }
	}

	public DataTemplate FooterTemplate
	{
		get => _footerTemplate;
		set { _footerTemplate = value; OnPropertyChanged(); }
	}

	public DataTemplate GroupHeaderTemplate
	{
		get => _groupHeaderTemplate;
		set { _groupHeaderTemplate = value; OnPropertyChanged(); }
	}

	public DataTemplate GroupFooterTemplate
	{
		get => _groupFooterTemplate;
		set { _groupFooterTemplate = value; OnPropertyChanged(); }
	}

	public DataTemplate ItemTemplate
	{
		get => _itemTemplate;
		set { _itemTemplate = value; OnPropertyChanged(); }
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

	public ItemsSourceType ItemsSourceType
	{
		get => _itemsSourceType;
		set
		{
			if (_itemsSourceType != value)
			{
				_itemsSourceType = value;
				OnPropertyChanged();
				OnPropertyChanged(nameof(ItemsSource));
				OnPropertyChanged(nameof(ShowAddRemoveButtons));
				SetItemTemplate();
			}
		}
	}

	public bool CanReorderItems
	{
		get => _canReorderItems;
		set
		{
			if (_canReorderItems != value)
			{
				_canReorderItems = value;
				OnPropertyChanged();
			}
		}
	}
	public bool CanMixGroups
	{
		get => _canMixGroups;
		set
		{
			if (_canMixGroups != value)
			{
				_canMixGroups = value;
				OnPropertyChanged();
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
			}
		}
	}

	public object ItemsSource
	{
		get
		{
			return ItemsSourceType switch
			{
				ItemsSourceType.ObservableCollectionT => _observableCollection,
				ItemsSourceType.ObservableCollection25T => _observableCollection25,
				ItemsSourceType.ObservableCollection5T => _observableCollection5,
				ItemsSourceType.ObservableCollectionT3 => _observableCollection3,
				ItemsSourceType.ObservableCollectionT2 => _observableCollection2,
				ItemsSourceType.GroupedListT => _groupedList,
				ItemsSourceType.GroupedListT2 => _groupedList2,
				ItemsSourceType.EmptyGroupedListT => _emptyGroupedList,
				ItemsSourceType.GroupedListT3 => _groupedList3,
				ItemsSourceType.EmptyObservableCollectionT => _emptyObservableCollection,
				ItemsSourceType.ObservableCollectionStringT => _observableCollectionString,
				ItemsSourceType.ObservableCollectionModelT => _observableCollectionModel,
				ItemsSourceType.GroupedListModelT => _groupedListModel,
				ItemsSourceType.GroupedListStringT => _groupedListString,
				ItemsSourceType.ListT => _list,
				ItemsSourceType.ListModelT => _listModel,
				ItemsSourceType.EmptyGroupedListModelT => _emptyGroupedListModel,
				ItemsSourceType.EmptyObservableCollectionModelT => _emptyObservableCollectionModel,
				ItemsSourceType.None => null,
				_ => null
			};
		}
	}

	public ItemSizingStrategy ItemSizingStrategy
	{
		get => _itemSizingStrategy;
		set
		{
			if (_itemSizingStrategy != value)
			{
				_itemSizingStrategy = value;
				OnPropertyChanged();
			}
		}
	}

	public ItemsUpdatingScrollMode ItemsUpdatingScrollMode
	{
		get => _itemsUpdatingScrollMode;
		set
		{
			if (_itemsUpdatingScrollMode != value)
			{
				_itemsUpdatingScrollMode = value;
				OnPropertyChanged();
			}
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
				OnPropertyChanged(nameof(SelectedItemText));
				OnPropertyChanged(nameof(SelectedItemsCount));
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
				OnPropertyChanged(nameof(SelectedItemsCount));
				OnPropertyChanged(nameof(SelectedItemText));
			}
		}
	}
	public int SelectedItemsCount
	{
		get
		{
			if (SelectionMode == SelectionMode.Single)
			{
				return SelectedItem != null ? 1 : 0;
			}
			else if (SelectionMode == SelectionMode.Multiple)
			{
				return SelectedItems?.Count ?? 0;
			}
			return 0;

		}
	}
	public string SelectedItemText
	{
		get
		{
			if (SelectionMode == SelectionMode.Single && SelectedItem is CollectionViewTestItem item)
			{
				return $"{item.Caption}";
			}
			else if (SelectionMode == SelectionMode.Multiple && SelectedItems?.Count > 0)
			{
				var selectedCaptions = SelectedItems
					.OfType<CollectionViewTestItem>()
					.Select(i => i.Caption);
				return string.Join(", ", selectedCaptions);
			}
			return "No items selected";
		}
	}

	public int SelectionChangedEventCount
	{
		get => _selectionChangedEventCount;
		set
		{
			_selectionChangedEventCount = value;
			OnPropertyChanged();
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

	public bool ItemsSourceStringItems
	{
		get => _itemsSourceStringItems;
		set
		{
			_itemsSourceStringItems = value;
			OnPropertyChanged();
		}
	}

	public bool IsHeaderStringSelected
	{
		get => _isHeaderStringSelected;
		set
		{
			_isHeaderStringSelected = value;
			OnPropertyChanged();
		}
	}

	public bool IsFooterStringSelected
	{
		get => _isFooterStringSelected;
		set
		{
			_isFooterStringSelected = value;
			OnPropertyChanged();
		}
	}

	public bool IsHeaderGridSelected
	{
		get => _isHeaderGridSelected;
		set
		{
			_isHeaderGridSelected = value;
			OnPropertyChanged();
		}
	}

	public bool IsFooterGridSelected
	{
		get => _isFooterGridSelected;
		set
		{
			_isFooterGridSelected = value;
			OnPropertyChanged();
		}
	}

	public bool IsHeaderTemplateViewSelected
	{
		get => _isHeaderTemplateViewSelected;
		set
		{
			_isHeaderTemplateViewSelected = value;
			OnPropertyChanged();
		}
	}

	public bool IsFooterTemplateViewSelected
	{
		get => _isFooterTemplateViewSelected;
		set
		{
			_isFooterTemplateViewSelected = value;
			OnPropertyChanged();
		}
	}

	public bool IsGroupHeaderTemplateViewSelected
	{
		get => _isGroupHeaderTemplateViewSelected;
		set
		{
			_isGroupHeaderTemplateViewSelected = value;
			OnPropertyChanged();
		}
	}

	public bool IsGroupFooterTemplateViewSelected
	{
		get => _isGroupFooterTemplateViewSelected;
		set
		{
			_isGroupFooterTemplateViewSelected = value;
			OnPropertyChanged();
		}
	}

	public bool IsEmptyViewStringSelected
	{
		get => _isEmptyViewStringSelected;
		set
		{
			_isEmptyViewStringSelected = value;
			OnPropertyChanged();
		}
	}

	public bool IsEmptyViewGridSelected
	{
		get => _isEmptyViewGridSelected;
		set
		{
			_isEmptyViewGridSelected = value;
			OnPropertyChanged();
		}
	}

	public bool IsEmptyViewTemplateSelected
	{
		get => _isEmptyViewTemplateSelected;
		set
		{
			_isEmptyViewTemplateSelected = value;
			OnPropertyChanged();
		}
	}

	public bool IsItemTemplateSelected
	{
		get => _isItemTemplateSelected;
		set
		{
			_isItemTemplateSelected = value;
			OnPropertyChanged();
		}
	}

	private void LoadItems()
	{
		_observableCollection = new ObservableCollection<CollectionViewTestItem>();
		AddItems(_observableCollection, 7, "Fruits");
		AddItems(_observableCollection, 7, "Vegetables");

		_observableCollection25 = new ObservableCollection<CollectionViewTestItem>();
		AddItems(_observableCollection25, 10, "Fruits");
		AddItems(_observableCollection25, 10, "Vegetables");

		_observableCollection5 = new ObservableCollection<CollectionViewTestItem>();
		AddItems(_observableCollection5, 5, "Fruits");

		_observableCollection2 = new ObservableCollection<ItemModel>();
		AddItems(_observableCollection2, 10);

		_observableCollectionModel = new ObservableCollection<CollectionViewTestModelItem>();
		AddItems(_observableCollectionModel, 2, "Group A");
		AddItems(_observableCollectionModel, 1, "Group B");

		_observableCollectionString = new ObservableCollection<CollectionViewTestItem>();
		AddItems(_observableCollectionString, 2, "Fruits");
		AddItems(_observableCollectionString, 2, "Vegetables");

		_list = new List<CollectionViewTestItem>();
		AddItems(_list, 2, "Fruits");
		AddItems(_list, 2, "Vegetables");

		_listModel = new List<CollectionViewTestModelItem>();
		AddItems(_listModel, 2, "Group A");

		_groupedList = new List<Grouping<string, CollectionViewTestItem>>
			{
				new Grouping<string, CollectionViewTestItem>("Fruits", new List<CollectionViewTestItem>()),
				new Grouping<string, CollectionViewTestItem>("Vegetables", new List<CollectionViewTestItem>())
			};
		AddItems(_groupedList[0], 4, "Fruits");
		AddItems(_groupedList[1], 4, "Vegetables");

		_observableCollection3 = new ObservableCollection<CollectionViewTestItem>();
		AddItems(_observableCollection3, 15, "Fruits");
		AddItems(_observableCollection3, 15, "Vegetables");

		_groupedList3 = new List<Grouping<string, CollectionViewTestItem>>
			{
				new Grouping<string, CollectionViewTestItem>("Fruits", new List<CollectionViewTestItem>()),
				new Grouping<string, CollectionViewTestItem>("Vegetables", new List<CollectionViewTestItem>())

			};
		AddItems(_groupedList3[0], 12, "Fruits");
		AddItems(_groupedList3[1], 12, "Vegetables");

		_groupedList2 = new List<Grouping<string, ItemModel>>
			{
				new Grouping<string, ItemModel>("Group1", new List<ItemModel>()),
				new Grouping<string, ItemModel>("Group2", new List<ItemModel>())
			};
		AddItems(_groupedList2[0], 3);
		AddItems(_groupedList2[1], 2);

		_groupedListString = new List<Grouping<string, CollectionViewTestItem>>
		{
			new Grouping<string, CollectionViewTestItem>("Fruits", new List<CollectionViewTestItem>()),
			new Grouping<string, CollectionViewTestItem>("Vegetables", new List<CollectionViewTestItem>())
		};
		AddItems(_groupedListString[0], 3, "Fruits");
		AddItems(_groupedListString[1], 3, "Vegetables");

		_groupedListModel = new List<Grouping<string, CollectionViewTestModelItem>>
		{
			new Grouping<string, CollectionViewTestModelItem>("Group A", new List<CollectionViewTestModelItem>()),
			new Grouping<string, CollectionViewTestModelItem>("Group B", new List<CollectionViewTestModelItem>())
		};
		AddItems(_groupedListModel[0], 1, "Group A");
		AddItems(_groupedListModel[1], 1, "Group B");

		_emptyGroupedList = new List<Grouping<string, CollectionViewTestItem>>();
		_emptyGroupedListModel = new List<Grouping<string, CollectionViewTestModelItem>>();
		_emptyObservableCollection = new ObservableCollection<CollectionViewTestItem>();
		_emptyObservableCollectionModel = new ObservableCollection<CollectionViewTestModelItem>();
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
			list.Add(new CollectionViewTestItem(items[n % items.Length], n));
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

	private void AddItems(IList<ItemModel> list, int count)
	{
		string loremParagraph = "Lorem ipsum dolor sit amet, consectetur adipiscing elit. Sed do eiusmod tempor incididunt ut labore et dolore magna aliqua. Ut enim ad minim veniam, quis nostrud exercitation ullamco laboris nisi ut aliquip ex ea commodo consequat. Duis aute irure dolor in reprehenderit in voluptate velit esse cillum dolore eu fugiat nulla pariatur. Excepteur sint occaecat cupidatat non proident, sunt in culpa qui officia deserunt mollit anim id est laborum.";
		string[] sentences = loremParagraph.Split(new[] { ". " }, StringSplitOptions.RemoveEmptyEntries);

		double fixedFontSize = 16;

		for (int i = 0; i < count; i++)
		{
			int sentenceCount = (i % sentences.Length) + 1;
			string text = string.Join(". ", sentences.Take(sentenceCount)) + ".";
			list.Add(new ItemModel(text, fixedFontSize));
		}
	}

	private void AddItem()
	{
		if (_addIndex >= _addSequenceFruits.Length)
			_addIndex = 0;

		var fruitName = _addSequenceFruits[_addIndex++];
		var newItem = new CollectionViewTestItem(fruitName, _addIndex - 1);

		if (ItemsSourceType == ItemsSourceType.ObservableCollectionT3)
		{
			_observableCollection3.Insert(0, newItem);
		}
		else if (ItemsSourceType == ItemsSourceType.GroupedListT3 && _groupedList3.Count > 0)
		{
			_groupedList3[0].Insert(0, newItem);
		}

		OnPropertyChanged(nameof(ItemsSource));
	}

	private void SetItemTemplate()
	{
		if (ItemsSourceType == ItemsSourceType.ObservableCollectionT2 || ItemsSourceType == ItemsSourceType.GroupedListT2)
		{

			ItemTemplate = new DataTemplate(() =>
			{
				var stackLayout = new StackLayout
				{
					BackgroundColor = Colors.LightBlue,
					Margin = new Thickness(1),

				};

				var label = new Label
				{
					TextColor = Colors.Black
				};

				label.SetBinding(Label.TextProperty, "Caption");
				label.SetBinding(Label.FontSizeProperty, "FontSize");

				stackLayout.Children.Add(label);

				return stackLayout;
			});
		}
		else if (ItemsSourceStringItems)
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
		else if (!ItemsSourceStringItems)
		{
			ItemTemplate = ExampleTemplates.PhotoTemplate();
		}
		else
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
	}

	public void AddSequentialItem()
	{
		string[] fruits = { "Kiwi", "Guava", "Chikoo", "Raseberry", "Papaya", "Pineapple", "Strawberry", "Blueberry", "Peach", "Cherry" };
		string[] groupA = { "green.png", "groceries.png", "oasis.jpg", "seth.png", "test.jpg", "vegetables.jpg" };

		string nextItem = ItemsSourceType switch
		{
			ItemsSourceType.ObservableCollectionStringT or ItemsSourceType.GroupedListStringT => fruits[fruitIndex++ % fruits.Length],
			ItemsSourceType.ObservableCollectionModelT or ItemsSourceType.GroupedListModelT => groupA[groupAIndex++ % groupA.Length],
			_ => string.Empty
		};

		switch (ItemsSourceType)
		{
			case ItemsSourceType.ObservableCollectionStringT:
				_observableCollectionString.Add(new CollectionViewTestItem(nextItem, _observableCollectionString.Count));
				break;

			case ItemsSourceType.ObservableCollectionModelT:
				_observableCollectionModel.Add(new CollectionViewTestModelItem(nextItem, nextItem, _observableCollectionModel.Count));
				break;

			case ItemsSourceType.GroupedListStringT:
				if (_groupedListString.Count > 1)
				{
					int targetGroupIndex = _groupedListString[0].Count <= _groupedListString[1].Count ? 0 : 1;
					_groupedListString[targetGroupIndex].Add(new CollectionViewTestItem(nextItem, _groupedListString[targetGroupIndex].Count));
				}
				else if (_groupedListString.Count > 0)
				{
					_groupedListString[0].Add(new CollectionViewTestItem(nextItem, _groupedListString[0].Count));
				}
				break;

			case ItemsSourceType.GroupedListModelT:
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
		}

		OnPropertyChanged(nameof(ItemsSource));
	}

	public void RemoveLastItem()
	{
		object deletedItem = null;
		switch (ItemsSourceType)
		{
			case ItemsSourceType.ObservableCollectionStringT:
				if (_observableCollectionString.Count > 0)
				{
					deletedItem = _observableCollectionString[^1];
					_observableCollectionString.RemoveAt(_observableCollectionString.Count - 1);
				}
				break;

			case ItemsSourceType.ObservableCollectionModelT:
				if (_observableCollectionModel.Count > 0)
				{
					deletedItem = _observableCollectionModel[^1];
					_observableCollectionModel.RemoveAt(_observableCollectionModel.Count - 1);
				}
				break;

			case ItemsSourceType.GroupedListStringT:
				if (_groupedListString.Count > 0)
				{
					int targetGroupIndex = (_groupedListString[0].Count > 0) ? 0 : 1;

					if (targetGroupIndex < _groupedListString.Count && _groupedListString[targetGroupIndex].Count > 0)
					{
						deletedItem = _groupedListString[targetGroupIndex][^1];
						_groupedListString[targetGroupIndex].RemoveAt(_groupedListString[targetGroupIndex].Count - 1);
					}
				}
				break;

			case ItemsSourceType.GroupedListModelT:
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

		switch (ItemsSourceType)
		{
			case ItemsSourceType.ObservableCollectionStringT:
				if (index >= 0 && index <= _observableCollectionString.Count)
				{
					_observableCollectionString.Insert(index, new CollectionViewTestItem(sequentialItem, index));
				}
				break;

			case ItemsSourceType.ObservableCollectionModelT:
				if (index >= 0 && index <= _observableCollectionModel.Count)
				{
					_observableCollectionModel.Insert(index, new CollectionViewTestModelItem(sequentialImageItem, sequentialImageItem, index));
				}
				break;

			case ItemsSourceType.GroupedListStringT:
				if (_groupedListString.Count > 0 && index >= 0 && index <= _groupedListString[0].Count)
				{
					_groupedListString[0].Insert(index, new CollectionViewTestItem(sequentialItem, index));
				}
				break;

			case ItemsSourceType.GroupedListModelT:
				if (_groupedListModel.Count > 0 && index >= 0 && index <= _groupedListModel[0].Count)
				{
					_groupedListModel[0].Insert(index, new CollectionViewTestModelItem(sequentialImageItem, sequentialImageItem, index));
				}
				break;
		}

		OnPropertyChanged(nameof(ItemsSource));
	}
	public void RemoveItemAtIndex(int index)
	{
		object deletedItem = null;

		switch (ItemsSourceType)
		{
			case ItemsSourceType.ObservableCollectionStringT:
				if (index >= 0 && index < _observableCollectionString.Count)
				{
					deletedItem = _observableCollectionString[index];
					_observableCollectionString.RemoveAt(index);
				}
				break;

			case ItemsSourceType.ObservableCollectionModelT:
				if (index >= 0 && index < _observableCollectionModel.Count)
				{
					deletedItem = _observableCollectionModel[index];
					_observableCollectionModel.RemoveAt(index);
				}
				break;

			case ItemsSourceType.GroupedListStringT:
				if (_groupedListString.Count > 0)
				{
					if (index >= 0 && index < _groupedListString[0].Count)
					{
						deletedItem = _groupedListString[0][index];
						_groupedListString[0].RemoveAt(index);
					}
					else if (_groupedListString.Count > 1 && index >= 0 && index < _groupedListString[1].Count)
					{
						deletedItem = _groupedListString[1][index];
						_groupedListString[1].RemoveAt(index);
					}
				}
				break;

			case ItemsSourceType.GroupedListModelT:
				if (_groupedListModel.Count > 0)
				{
					if (index >= 0 && index < _groupedListModel[0].Count)
					{
						deletedItem = _groupedListModel[0][index];
						_groupedListModel[0].RemoveAt(index);
					}
					else if (_groupedListModel.Count > 1 && index >= 0 && index < _groupedListModel[1].Count)
					{
						deletedItem = _groupedListModel[1][index];
						_groupedListModel[1].RemoveAt(index);
					}
				}
				break;
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

	private void OnSelectedItemsChanged(object sender, NotifyCollectionChangedEventArgs e)
	{
		OnPropertyChanged(nameof(SelectedItemsCount));
		OnPropertyChanged(nameof(SelectedItemText));
	}

	protected void OnPropertyChanged([CallerMemberName] string propertyName = null)
	{
		if (propertyName == nameof(IsGrouped))
		{
			OnPropertyChanged(nameof(ItemsSource));
		}

		PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
	}

	public class CustomDataTemplateSelector : DataTemplateSelector
	{
		public DataTemplate Template1 { get; set; }
		public DataTemplate Template2 { get; set; }

		protected override DataTemplate OnSelectTemplate(object item, BindableObject container)
		{
			if (item is CollectionViewTestItem testItem)
			{
				return testItem.Index % 2 == 0 ? Template1 : Template2;
			}

			return Template1;
		}
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

	public class ItemModel
	{
		public string Caption { get; set; }
		public double FontSize { get; set; }

		public ItemModel(string caption, double fontSize)
		{
			Caption = caption;
			FontSize = fontSize;
		}
	}
}