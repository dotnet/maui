using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Controls.Shapes;
using Microsoft.Maui.Graphics;

namespace Maui.Controls.Sample;

public class IndicatorViewViewModel : INotifyPropertyChanged
{
	private ObservableCollection<IndicatorViewCarouselItem> _carouselItems;
	private bool _hideSingle = true;
	private Color _indicatorColor = Colors.LightGrey;
	private double _indicatorSize = 6.0;
	private IndicatorShape _indicatorsShape = IndicatorShape.Circle;
	private int _maximumVisible;
	private int _position = 0;
	private int _count;
	private Color _selectedIndicatorColor = Colors.Black;
	private DataTemplate _indicatorTemplate;
	private string _activeTemplate = "Icon";

	private string _currentTemplateName = "Default";
	private FlowDirection _flowDirection = FlowDirection.LeftToRight;
	private bool _isEnabled = true;
	private bool _isVisible = true;
	private bool _hasShadow = false;
	private IndicatorViewCarouselItem _currentItem;

	public IndicatorViewViewModel()
	{
		InitializeCarouselItems();
	}

	private void InitializeCarouselItems()
	{
		_carouselItems = new ObservableCollection<IndicatorViewCarouselItem>
		{
			new IndicatorViewCarouselItem { Title = "Item 1", Description = "First carousel item", Color = Colors.Red },
			new IndicatorViewCarouselItem { Title = "Item 2", Description = "Second carousel item", Color = Colors.Blue },
			new IndicatorViewCarouselItem { Title = "Item 3", Description = "Third carousel item", Color = Colors.Green },
			new IndicatorViewCarouselItem { Title = "Item 4", Description = "Fourth carousel item", Color = Colors.Orange },
			new IndicatorViewCarouselItem { Title = "Item 5", Description = "Fifth carousel item", Color = Colors.Purple },
		};
		_carouselItems.CollectionChanged += CarouselItems_CollectionChanged;

		// Initialize Count, MaximumVisible, and CurrentItem
		Count = _carouselItems.Count;
		MaximumVisible = _carouselItems.Count;
		CurrentItem = _carouselItems.FirstOrDefault();
	}

	private void CarouselItems_CollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
	{
		Count = _carouselItems.Count;
		MaximumVisible = _carouselItems.Count;

		if (_carouselItems.Count > 0)
		{
			CurrentItem = _carouselItems.FirstOrDefault();
			Position = 0;
		}
		else
		{
			CurrentItem = null;
			Position = 0;
		}
	}

	public ObservableCollection<IndicatorViewCarouselItem> CarouselItems
	{
		get => _carouselItems;
		set
		{
			if (_carouselItems != value)
			{
				if (_carouselItems != null)
					_carouselItems.CollectionChanged -= CarouselItems_CollectionChanged;

				_carouselItems = value;

				if (_carouselItems != null)
					_carouselItems.CollectionChanged += CarouselItems_CollectionChanged;

				OnPropertyChanged();
				Count = _carouselItems.Count;
				MaximumVisible = _carouselItems.Count;
			}
		}
	}

	public IndicatorViewCarouselItem CurrentItem
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

	public int Count
	{
		get => _count;
		set { if (_count != value) { _count = value; OnPropertyChanged(); } }
	}

	public int Position
	{
		get => _position;
		set { if (_position != value) { _position = value; OnPropertyChanged(); } }
	}

	public bool HideSingle
	{
		get => _hideSingle;
		set { if (_hideSingle != value) { _hideSingle = value; OnPropertyChanged(); } }
	}

	public Color IndicatorColor
	{
		get => _indicatorColor;
		set { if (_indicatorColor != value) { _indicatorColor = value; OnPropertyChanged(); } }
	}

	public double IndicatorSize
	{
		get => _indicatorSize;
		set { if (_indicatorSize != value) { _indicatorSize = value; OnPropertyChanged(); } }
	}

	public IndicatorShape IndicatorsShape
	{
		get => _indicatorsShape;
		set { if (_indicatorsShape != value) { _indicatorsShape = value; OnPropertyChanged(); } }
	}

	public int MaximumVisible
	{
		get => _maximumVisible;
		set
		{
			if (_maximumVisible != value)
			{
				_maximumVisible = value;
				OnPropertyChanged();
			}
		}
	}


	public Color SelectedIndicatorColor
	{
		get => _selectedIndicatorColor;
		set { if (_selectedIndicatorColor != value) { _selectedIndicatorColor = value; OnPropertyChanged(); } }
	}

	public string CurrentTemplateName
	{
		get => _currentTemplateName;
		set
		{
			if (_currentTemplateName != value)
			{
				_currentTemplateName = value;
				OnPropertyChanged();
			}
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

	public bool IsEnabled
	{
		get => _isEnabled;
		set
		{
			if (_isEnabled != value)
			{
				_isEnabled = value;
				OnPropertyChanged();
			}
		}
	}

	public bool IsVisible
	{
		get => _isVisible;
		set
		{
			if (_isVisible != value)
			{
				_isVisible = value;
				OnPropertyChanged();
			}
		}
	}

	public bool HasShadow
	{
		get => _hasShadow;
		set
		{
			if (_hasShadow != value)
			{
				_hasShadow = value;
				OnPropertyChanged();
				OnPropertyChanged(nameof(Shadow));
			}
		}
	}

	public Shadow Shadow => _hasShadow ? new Shadow
	{
		Brush = Brush.Black,
		Offset = new Point(5, 5),
		Radius = 5,
		Opacity = 0.5f
	} : null;

	public DataTemplate IndicatorTemplate
	{
		get => _indicatorTemplate;
		set
		{
			if (_indicatorTemplate != value)
			{
				_indicatorTemplate = value;
				OnPropertyChanged();
			}
		}
	}

	public string ActiveTemplate
	{
		get => _activeTemplate;
		set
		{
			_activeTemplate = value;
			OnPropertyChanged();
		}
	}

	public void SetIconTemplate()
	{
		IndicatorTemplate = new DataTemplate(() =>
		{
			return new Label
			{
				Text = "\uf30c",
				FontFamily = "ionicons",
				HorizontalOptions = LayoutOptions.Center,
				VerticalOptions = LayoutOptions.Center
			};
		});
		ActiveTemplate = "Icon";
	}

	public void SetStarTemplate()
	{
		IndicatorTemplate = new DataTemplate(() =>
		{
			return new Label
			{
				Text = "\uf4b3",
				FontFamily = "ionicons",
				HorizontalOptions = LayoutOptions.Center,
				VerticalOptions = LayoutOptions.Center
			};
		});
		ActiveTemplate = "Star";
	}

	public void SetHeartTemplate()
	{
		IndicatorTemplate = new DataTemplate(() =>
		{
			return new Label
			{
				Text = "\u2665",
				FontFamily = "ionicons",
				HorizontalOptions = LayoutOptions.Center,
				VerticalOptions = LayoutOptions.Center
			};
		});
		ActiveTemplate = "Heart";
	}
	public event PropertyChangedEventHandler PropertyChanged;
	protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
		=> PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
}

public class IndicatorViewCarouselItem
{
	public string Title { get; set; }
	public string Description { get; set; }
	public Color Color { get; set; }
}