using System;
using System.ComponentModel;
using System.Globalization;
using System.Runtime.CompilerServices;

namespace Maui.Controls.Sample;

public class TimePickerViewModel : INotifyPropertyChanged
{
	private double _characterSpacing = 0.0d;
	private FlowDirection _flowDirection = FlowDirection.MatchParent;
	private FontAttributes _fontAttributes;
	private string _fontFamily;
	private double _fontSize = -1.0d;
	private string _format = "hh:mm tt";
	private bool _fontAutoScalingEnabled = true;
	private bool _isEnabled = true;
	private bool _isOpen;
	private bool _isVisible = true;
	private Shadow _shadow;
	private TimeSpan? _time = new TimeSpan(10, 0, 0);
	private Color _textColor;
	private CultureInfo _culture;

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
	public string Format
	{
		get => _format;
		set
		{
			if (_format != value)
			{
				_format = value;
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

	public bool IsOpen
	{
		get => _isOpen;
		set
		{
			if (_isOpen != value)
			{
				_isOpen = value;
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

	public TimeSpan? Time
	{
		get => _time;
		set
		{
			if (_time != value)
			{
				_time = value;
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

	public CultureInfo Culture
	{
		get => _culture;
		set
		{
			if (_culture != value)
			{
				_culture = value;
				OnPropertyChanged();
			}
		}
	}

	public event PropertyChangedEventHandler PropertyChanged;

	public void ResetToDefaults()
	{
		_characterSpacing = 0.0d;
		_flowDirection = FlowDirection.MatchParent;
		_fontAttributes = FontAttributes.None;
		_fontFamily = null;
		_fontSize = -1.0d;
		_format = "hh:mm tt";
		_fontAutoScalingEnabled = true;
		_isEnabled = true;
		_isOpen = false;
		_isVisible = true;
		_shadow = null;
		_time = new TimeSpan(10, 0, 0);
		_textColor = null;
		_culture = new CultureInfo("en-US");

		OnPropertyChanged(nameof(CharacterSpacing));
		OnPropertyChanged(nameof(FlowDirection));
		OnPropertyChanged(nameof(FontAttributes));
		OnPropertyChanged(nameof(FontFamily));
		OnPropertyChanged(nameof(FontSize));
		OnPropertyChanged(nameof(Format));
		OnPropertyChanged(nameof(FontAutoScalingEnabled));
		OnPropertyChanged(nameof(IsEnabled));
		OnPropertyChanged(nameof(IsOpen));
		OnPropertyChanged(nameof(IsVisible));
		OnPropertyChanged(nameof(Shadow));
		OnPropertyChanged(nameof(Time));
		OnPropertyChanged(nameof(TextColor));
		OnPropertyChanged(nameof(Culture));
	}

	protected void OnPropertyChanged([CallerMemberName] string propertyName = null)
	{
		PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
	}

	public TimePickerViewModel()
	{
		Culture = new CultureInfo("en-US");
	}
}
