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
	private Color _textColor = Colors.Black;
	private TextTransform _textTransform = TextTransform.None;
	private string _title = "Picker Title";
	private Color _titleColor = Colors.Black;
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

	public void ResetToDefaults()
	{
		_characterSpacing = 0;
		_flowDirection = FlowDirection.LeftToRight;
		_fontAttributes = FontAttributes.None;
		_fontAutoScalingEnabled = true;
		_fontFamily = null;
		_fontSize = -1;
		_horizontalTextAlignment = TextAlignment.Start;
		_isEnabled = true;
		// _itemDisplayBinding = null;
		_isVisible = true;
		_selectedIndex = -1;
		_selectedItem = null;
		_shadow = null;
		_textColor = Colors.Black;
		_textTransform = TextTransform.None;
		_title = "Picker Title";
		_titleColor = Colors.Black;
		_verticalTextAlignment = TextAlignment.Center;

		OnPropertyChanged(nameof(CharacterSpacing));
		OnPropertyChanged(nameof(FlowDirection));
		OnPropertyChanged(nameof(FontAttributes));
		OnPropertyChanged(nameof(FontAutoScalingEnabled));
		OnPropertyChanged(nameof(FontFamily));
		OnPropertyChanged(nameof(FontSize));
		OnPropertyChanged(nameof(HorizontalTextAlignment));
		OnPropertyChanged(nameof(IsEnabled));
		// OnPropertyChanged(nameof(ItemDisplayBinding));
		OnPropertyChanged(nameof(IsVisible));
		OnPropertyChanged(nameof(SelectedIndex));
		OnPropertyChanged(nameof(SelectedItem));
		OnPropertyChanged(nameof(Shadow));
		OnPropertyChanged(nameof(TextColor));
		OnPropertyChanged(nameof(TextTransform));
		OnPropertyChanged(nameof(Title));
		OnPropertyChanged(nameof(TitleColor));
		OnPropertyChanged(nameof(VerticalTextAlignment));
	}
}