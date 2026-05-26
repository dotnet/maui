using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Windows.Input;
using Microsoft.Maui.Controls;

namespace Maui.Controls.Sample;

public class SwipeViewViewModel : INotifyPropertyChanged
{
	private bool _isEnabled = true;
	private bool _isVisible = true;
	private Color _backgroundColor = Color.FromArgb("#F0F0F0");
	private Color _swipeItemsBackgroundColor = Color.FromArgb("#6A5ACD");
	private FlowDirection _flowDirection = FlowDirection.LeftToRight;
	private double _threshold = 100;
	private bool _hasShadow = false;
	private Shadow _swipeViewShadow = null;
	private SwipeMode _swipeMode = SwipeMode.Reveal;
	private SwipeBehaviorOnInvoked _swipeBehavior = SwipeBehaviorOnInvoked.Auto;
	private string _eventInvokedText = "Event not invoked yet";
	private string _swipeStartedText = "Swipe Started: ";
	private string _swipeChangingText = "Swipe Changing: ";
	private string _swipeEndedText = "Swipe Ended: ";
	private string _selectedSwipeItemType = "Label";
	private string _selectedContentType = "Label";

	public event PropertyChangedEventHandler PropertyChanged;
	public event Action<OpenSwipeItem> RequestOpen;
	public event Action RequestClose;

	public SwipeViewViewModel()
	{
		OpenLeftCommand = new Command(() => RequestOpen?.Invoke(OpenSwipeItem.LeftItems));
		OpenRightCommand = new Command(() => RequestOpen?.Invoke(OpenSwipeItem.RightItems));
		OpenTopCommand = new Command(() => RequestOpen?.Invoke(OpenSwipeItem.TopItems));
		OpenBottomCommand = new Command(() => RequestOpen?.Invoke(OpenSwipeItem.BottomItems));
		CloseCommand = new Command(() => RequestClose?.Invoke());
	}

	public bool IsEnabled
	{
		get => _isEnabled;
		set { _isEnabled = value; OnPropertyChanged(nameof(IsEnabled)); }
	}

	public bool IsVisible
	{
		get => _isVisible;
		set { _isVisible = value; OnPropertyChanged(nameof(IsVisible)); }
	}

	public Color BackgroundColor
	{
		get => _backgroundColor;
		set { _backgroundColor = value; OnPropertyChanged(nameof(BackgroundColor)); }
	}

	public Color SwipeItemsBackgroundColor
	{
		get => _swipeItemsBackgroundColor;
		set { _swipeItemsBackgroundColor = value; OnPropertyChanged(nameof(SwipeItemsBackgroundColor)); }
	}

	public FlowDirection FlowDirection
	{
		get => _flowDirection;
		set { _flowDirection = value; OnPropertyChanged(nameof(FlowDirection)); }
	}
	public bool HasShadow
	{
		get => _hasShadow;
		set
		{
			if (_hasShadow != value)
			{
				_hasShadow = value;
				SwipeViewShadow = value
					? new Shadow
					{
						Radius = 10,
						Opacity = 1.0f,
						Brush = Colors.Black.AsPaint(),
						Offset = new Point(5, 5)
					}
					: null;
				OnPropertyChanged(nameof(HasShadow));
			}
		}
	}

	public Shadow SwipeViewShadow
	{
		get => _swipeViewShadow;
		set
		{
			if (_swipeViewShadow != value)
			{
				_swipeViewShadow = value;
				OnPropertyChanged(nameof(SwipeViewShadow));
			}
		}
	}

	public double Threshold
	{
		get => _threshold;
		set { _threshold = value; OnPropertyChanged(nameof(Threshold)); }
	}

	public SwipeMode SwipeMode
	{
		get => _swipeMode;
		set { _swipeMode = value; OnPropertyChanged(nameof(SwipeMode)); }
	}

	public SwipeBehaviorOnInvoked SwipeBehaviorOnInvoked
	{
		get => _swipeBehavior;
		set { _swipeBehavior = value; OnPropertyChanged(nameof(SwipeBehaviorOnInvoked)); }
	}

	public string EventInvokedText
	{
		get => _eventInvokedText;
		set { _eventInvokedText = value; OnPropertyChanged(nameof(EventInvokedText)); }
	}

	public string SwipeStartedText
	{
		get => _swipeStartedText;
		set { _swipeStartedText = value; OnPropertyChanged(nameof(SwipeStartedText)); }
	}

	public string SwipeChangingText
	{
		get => _swipeChangingText;
		set { _swipeChangingText = value; OnPropertyChanged(nameof(SwipeChangingText)); }
	}

	public string SwipeEndedText
	{
		get => _swipeEndedText;
		set { _swipeEndedText = value; OnPropertyChanged(nameof(SwipeEndedText)); }
	}

	public string SelectedContentType
	{
		get => _selectedContentType;
		set
		{
			if (_selectedContentType != value)
			{
				_selectedContentType = value;
				OnPropertyChanged(nameof(SelectedContentType));
			}
		}
	}

	public string SelectedSwipeItemType
	{
		get => _selectedSwipeItemType;
		set
		{
			if (_selectedSwipeItemType != value)
			{
				_selectedSwipeItemType = value;
				OnPropertyChanged(nameof(SelectedSwipeItemType));
			}
		}
	}


	public ICommand OpenLeftCommand { get; }
	public ICommand OpenRightCommand { get; }
	public ICommand OpenTopCommand { get; }
	public ICommand OpenBottomCommand { get; }
	public ICommand CloseCommand { get; }

	public ObservableCollection<ItemModel> Items { get; set; } = new()
	{
		new ItemModel { Title = "Item 1" },
		new ItemModel { Title = "Item 2" },
		new ItemModel { Title = "Item 3" },
		new ItemModel { Title = "Item 4" },
		new ItemModel { Title = "Item 5" },
		new ItemModel { Title = "Item 6" }
	};

	void OnPropertyChanged(string name) =>
		PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));

	public class ItemModel
	{
		public string Title { get; set; } = string.Empty;
	}
}