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
	GroupedListT3
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
	private SelectionMode _selectionMode = SelectionMode.None;
	private object _selectedItem;
	private ObservableCollection<object> _selectedItems = new ObservableCollection<object>();
	private int _selectionChangedEventCount = 0;
	private string _previousSelectionText;
	private string _currentSelectionText;

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


	private void LoadItems()
	{
		_observableCollection = new ObservableCollection<CollectionViewTestItem>();
		AddItems(_observableCollection, 7, "Fruits");
		AddItems(_observableCollection, 7, "Vegetables");

		_observableCollection25 = new ObservableCollection<CollectionViewTestItem>();
		_observableCollection5 = new ObservableCollection<CollectionViewTestItem>();
		AddItems(_observableCollection5, 5, "Fruits");
		AddItems(_observableCollection25, 10, "Fruits");
		AddItems(_observableCollection25, 10, "Vegetables");

		_emptyObservableCollection = new ObservableCollection<CollectionViewTestItem>();

		_groupedList = new List<Grouping<string, CollectionViewTestItem>>
			{
				new Grouping<string, CollectionViewTestItem>("Fruits", new List<CollectionViewTestItem>()),
				new Grouping<string, CollectionViewTestItem>("Vegetables", new List<CollectionViewTestItem>())
			};

		AddItems(_groupedList[0], 4, "Fruits");
		AddItems(_groupedList[1], 4, "Vegetables");

		_emptyGroupedList = new List<Grouping<string, CollectionViewTestItem>>();

		_observableCollection2 = new ObservableCollection<ItemModel>();
		AddItems2(_observableCollection2, 10);

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
		AddItems2(_groupedList2[0], 3);
		AddItems2(_groupedList2[1], 2);

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

	private void AddItems2(IList<ItemModel> list, int count)
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