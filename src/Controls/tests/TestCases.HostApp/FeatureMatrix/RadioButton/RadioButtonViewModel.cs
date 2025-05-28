using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using Microsoft.Maui.Controls;

namespace Maui.Controls.Sample
{
	public class RadioButtonViewModel : INotifyPropertyChanged
	{
		private string _content = "Radio Button Content";
		private bool _isChecked = false;
		private string _groupName = "Group1";
		private object _value = "Value1";
		private Color _borderColor = Color.FromRgba(1, 122, 255, 255);
		private double _borderWidth = 1;
		private double _characterSpacing = 0;
		private int _cornerRadius = 0;
		private FontAttributes _fontAttributes = FontAttributes.None;
		private bool _fontAutoScalingEnabled = true;
		private string _fontFamily = null;
		private double _fontSize = 14;
		private Color _textColor = Colors.Black;
		private TextTransform _textTransform = TextTransform.None;

		public event PropertyChangedEventHandler PropertyChanged;

		public RadioButtonViewModel()
		{
		}

		public string Content
		{
			get => _content;
			set
			{
				if (_content != value)
				{
					_content = value;
					OnPropertyChanged();
				}
			}
		}

		public bool IsChecked
		{
			get => _isChecked;
			set
			{
				if (_isChecked != value)
				{
					_isChecked = value;
					OnPropertyChanged();
				}
			}
		}

		public string GroupName
		{
			get => _groupName;
			set
			{
				if (_groupName != value)
				{
					_groupName = value;
					OnPropertyChanged();
				}
			}
		}

		public object Value
		{
			get => _value;
			set
			{
				if (_value != value)
				{
					_value = value;
					OnPropertyChanged();
				}
			}
		}

		public Color BorderColor
		{
			get => _borderColor;
			set
			{
				if (_borderColor != value)
				{
					_borderColor = value;
					OnPropertyChanged();
				}
			}
		}

		public double BorderWidth
		{
			get => _borderWidth;
			set
			{
				if (_borderWidth != value)
				{
					_borderWidth = value;
					OnPropertyChanged();
				}
			}
		}

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

		public int CornerRadius
		{
			get => _cornerRadius;
			set
			{
				if (_cornerRadius != value)
				{
					_cornerRadius = value;
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

		protected void OnPropertyChanged([CallerMemberName] string propertyName = null)
		{
			PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
		}
	}
}
