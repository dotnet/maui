using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace Maui.Controls.Sample;

public class PickerViewModel : INotifyPropertyChanged
{
	private double _characterSpacing = 0;
	private FlowDirection _flowDirection = FlowDirection.LeftToRight;
	private FontAttributes _fontAttributes = FontAttributes.None;
	private bool _fontAutoScalingEnabled = true;
	private string _fontFamily = null;
	private double _fontSize = -1;
	private TextAlignment _horizontalTextAlignment = TextAlignment.Start;
	private bool _isEnabled = true;
	private BindingBase _itemDisplayBinding = null;
	private bool _isVisible = true;
	private int _selectedIndex = -1;
	private object _selectedItem = null;
	private Shadow _shadow = null;
	private Color _textColor = null;
	private TextTransform _textTransform = TextTransform.None;
	private string _title = "Select an item";
	private Color _titleColor = null;
	private TextAlignment _verticalTextAlignment = TextAlignment.Center;

	public event PropertyChangedEventHandler PropertyChanged;

	public PickerViewModel()
	{
		InitializeItemsSource();
	}

	public class PickerDataItem
	{
		public string Name { get; set; }
		public string Description { get; set; }
		public override string ToString() => $"{Name} - {Description}";
	}

	private void InitializeItemsSource()
	{
		ItemsSource = new ObservableCollection<object>
			{
				new PickerDataItem { Name = "Option 1", Description = "First option" },
				new PickerDataItem { Name = "Option 2", Description = "Second option" },
				new PickerDataItem { Name = "Option 3", Description = "Third option" },
				new PickerDataItem { Name = "Option 4", Description = "Fourth option" },
				new PickerDataItem { Name = "Option 5", Description = "Fifth option" }
			};
	}

	public ObservableCollection<object> ItemsSource { get; private set; }

	public double CharacterSpacing
	{
		get => _characterSpacing;
		set
		{
			if (_characterSpacing != value)
			{
				_characterSpacing = value;
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

	public FontAttributes FontAttributes
	{
		get => _fontAttributes;
		set
		{
			if (_fontAttributes != value)
			{
				_fontAttributes = value;
				OnPropertyChanged();
			}
		}
	}

	public bool FontAutoScalingEnabled
	{
		get => _fontAutoScalingEnabled;
		set
		{
			if (_fontAutoScalingEnabled != value)
			{
				_fontAutoScalingEnabled = value;
				OnPropertyChanged();
			}
		}
	}

	public string FontFamily
	{
		get => _fontFamily;
		set
		{
			if (_fontFamily != value)
			{
				_fontFamily = value;
				OnPropertyChanged();
			}
		}
	}

	public double FontSize
	{
		get => _fontSize;
		set
		{
			if (_fontSize != value)
			{
				_fontSize = value;
				OnPropertyChanged();
			}
		}
	}

	public TextAlignment HorizontalTextAlignment
	{
		get => _horizontalTextAlignment;
		set
		{
			if (_horizontalTextAlignment != value)
			{
				_horizontalTextAlignment = value;
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

	public BindingBase ItemDisplayBinding
	{
		get => _itemDisplayBinding;
		set
		{
			if (_itemDisplayBinding != value)
			{
				_itemDisplayBinding = value;
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

	public int SelectedIndex
	{
		get => _selectedIndex;
		set
		{
			if (_selectedIndex != value)
			{
				_selectedIndex = value;
				OnPropertyChanged();

				// Update SelectedItem when SelectedIndex changes
				if (_selectedIndex >= 0 && _selectedIndex < ItemsSource.Count)
				{
					_selectedItem = ItemsSource[_selectedIndex];
					OnPropertyChanged(nameof(SelectedItem));
				}
				else
				{
					_selectedItem = null;
					OnPropertyChanged(nameof(SelectedItem));
				}
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

				// Update SelectedIndex when SelectedItem changes
				if (_selectedItem != null && ItemsSource.Contains(_selectedItem))
				{
					_selectedIndex = ItemsSource.IndexOf(_selectedItem);
					OnPropertyChanged(nameof(SelectedIndex));
				}
				else
				{
					_selectedIndex = -1;
					OnPropertyChanged(nameof(SelectedIndex));
				}
			}
		}
	}

	public Shadow Shadow
	{
		get => _shadow;
		set
		{
			if (_shadow != value)
			{
				_shadow = value;
				OnPropertyChanged();
			}
		}
	}

	public Color TextColor
	{
		get => _textColor;
		set
		{
			if (_textColor != value)
			{
				_textColor = value;
				OnPropertyChanged();
			}
		}
	}

	public TextTransform TextTransform
	{
		get => _textTransform;
		set
		{
			if (_textTransform != value)
			{
				_textTransform = value;
				OnPropertyChanged();
			}
		}
	}

	public string Title
	{
		get => _title;
		set
		{
			if (_title != value)
			{
				_title = value;
				OnPropertyChanged();
			}
		}
	}

	public Color TitleColor
	{
		get => _titleColor;
		set
		{
			if (_titleColor != value)
			{
				_titleColor = value;
				OnPropertyChanged();
			}
		}
	}

	public TextAlignment VerticalTextAlignment
	{
		get => _verticalTextAlignment;
		set
		{
			if (_verticalTextAlignment != value)
			{
				_verticalTextAlignment = value;
				OnPropertyChanged();
			}
		}
	}

	protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
	{
		PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
	}
}