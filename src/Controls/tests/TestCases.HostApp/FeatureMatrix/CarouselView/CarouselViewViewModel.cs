using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Windows.Input;

namespace Maui.Controls.Sample;

public enum CarouselItemsSourceType
{
	None,
	ObservableCollectionT,
}

public class CarouselViewViewModel : INotifyPropertyChanged
{
	private object _emptyView;
	private DataTemplate _emptyViewTemplate;
	private DataTemplate _itemTemplate;
	private CarouselItemsSourceType _itemsSourceType = CarouselItemsSourceType.ObservableCollectionT;
	private bool _isLoopEnabled = false;
	private bool _isSwipeEnabled = true;
	private Thickness _peekAreaInsets;
	private int _thickness;
	private int _position;
	private bool _isIndicatorViewVisible = true;
	private IItemsLayout _itemsLayout = new LinearItemsLayout(ItemsLayoutOrientation.Horizontal);
	private ItemsUpdatingScrollMode _itemsUpdatingScrollMode;
	private string _currentItem;
	private string _currentItemText;
	private string _previousItemText;
	private string _previousItemPosition;
	private int _currentPosition;
	private int _previousPosition;
	private FlowDirection _flowDirection = FlowDirection.LeftToRight;

	public CarouselViewViewModel()
	{
		LoadItems();
		AddItemCommand = new Command(AddItem);

		ItemTemplate = new DataTemplate(() =>
		{
			var label = new Label
			{
				HorizontalOptions = LayoutOptions.Center,
				VerticalOptions = LayoutOptions.Center,
			};
			label.SetBinding(Label.TextProperty, ".");

			var stack = new StackLayout
			{
				Padding = new Thickness(10),
				HorizontalOptions = LayoutOptions.Center,
				VerticalOptions = LayoutOptions.Center,
				Children = { label }
			};

			return stack;
		});

	}

	public event PropertyChangedEventHandler PropertyChanged;
	public ICommand AddItemCommand { get; }
	public ObservableCollection<string> Items { get; private set; }

	public object EmptyView
	{
		get => _emptyView;
		set { _emptyView = value; OnPropertyChanged(); }
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

	public CarouselItemsSourceType ItemsSourceType
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

	public bool Loop
	{
		get => _isLoopEnabled;
		set
		{
			if (_isLoopEnabled != value)
			{
				_isLoopEnabled = value;
				OnPropertyChanged();
			}
		}
	}

	public bool IsSwipeEnabled
	{
		get => _isSwipeEnabled;
		set
		{
			if (_isSwipeEnabled != value)
			{
				_isSwipeEnabled = value;
				OnPropertyChanged();
			}
		}
	}

	public bool IsIndicatorViewVisible
	{
		get => _isIndicatorViewVisible;
		set
		{
			if (_isIndicatorViewVisible != value)
			{
				_isIndicatorViewVisible = value;
				OnPropertyChanged();
			}
		}
	}

	public Thickness PeekAreaInsets
	{
		get => _peekAreaInsets;
		set
		{
			if (_peekAreaInsets != value)
			{
				_peekAreaInsets = value;
				OnPropertyChanged();
			}
		}
	}

	public int Thickness
	{
		get => _thickness;
		set
		{
			if (_thickness != value)
			{
				_thickness = value;
				PeekAreaInsets = new Thickness(value);
				OnPropertyChanged();
			}
		}
	}

	public int Position
	{
		get => _position;
		set
		{
			if (_position != value)
			{
				_position = value;
				OnPropertyChanged();
			}
		}
	}

	public string CurrentItem
	{
		get => _currentItem;
		set
		{
			if (_currentItem != value)
			{
				_currentItem = value;
				OnPropertyChanged();
			}
		}
	}

	public string CurrentItemText
	{
		get => _currentItemText;
		set
		{
			if (_currentItemText != value)
			{
				_currentItemText = value;
				OnPropertyChanged(nameof(CurrentItemText));
			}
		}
	}

	public string PreviousItemText
	{
		get => _previousItemText;
		set
		{
			if (_previousItemText != value)
			{
				_previousItemText = value;
				OnPropertyChanged(nameof(PreviousItemText));
			}
		}
	}

	public string PreviousItemPosition
	{
		get => _previousItemPosition;
		set
		{
			if (_previousItemPosition != value)
			{
				_previousItemPosition = value;
				OnPropertyChanged(nameof(PreviousItemPosition));
			}
		}
	}

	public int CurrentPosition
	{
		get => _currentPosition;
		set
		{
			if (_currentPosition != value)
			{
				_previousPosition = _currentPosition;
				_currentPosition = value;
				OnPropertyChanged(nameof(CurrentPosition));
				OnPropertyChanged(nameof(PreviousPosition));
			}
		}
	}

	public int PreviousPosition => _previousPosition;

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

	public object ItemsSource
	{
		get
		{
			return ItemsSourceType switch
			{
				CarouselItemsSourceType.ObservableCollectionT => Items,
				CarouselItemsSourceType.None => null,
				_ => Items
			};
		}
	}

	public FlowDirection FlowDirection
	{
		get => _flowDirection;
		set
		{
			if (_flowDirection != value)
			{
				_flowDirection = value;
				OnPropertyChanged();
			}
		}
	}

	private void LoadItems()
	{
		Items = new ObservableCollection<string>
			{
				"Item 1",
				"Item 2",
				"Item 3",
				"Item 4",
				"Item 5"
			};
	}

	private void AddItem()
	{
		Items.Insert(0, $"Item {Items.Count + 1}");
		OnPropertyChanged(nameof(Items));
	}

	protected void OnPropertyChanged([CallerMemberName] string propertyName = null)
	{
		PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
	}
}