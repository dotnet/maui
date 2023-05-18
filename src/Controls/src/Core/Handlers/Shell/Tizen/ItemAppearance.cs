using System.ComponentModel;
using System.Runtime.CompilerServices;
using GColor = Microsoft.Maui.Graphics.Color;

namespace Microsoft.Maui.Controls.Platform
{
	class ItemAppearance : INotifyPropertyChanged
	{
		GColor? _foregroundColor;
		GColor? _backgroundColor;
		GColor? _titleColor;
		GColor? _unselectedColor;

		public GColor? ForegroundColor
		{
			get
			{
				return _foregroundColor;
			}
			set
			{
				if (_foregroundColor != value)
				{
					_foregroundColor = value;
					NotifyPropertyChanged();
				}
			}
		}

		public GColor? BackgroundColor
		{
			get
			{
				return _backgroundColor;
			}
			set
			{
				if (_backgroundColor != value)
				{
					_backgroundColor = value;
					NotifyPropertyChanged();
				}
			}
		}


		public GColor? TitleColor
		{
			get
			{
				return _titleColor;
			}
			set
			{
				if (_titleColor != value)
				{
					_titleColor = value;
					NotifyPropertyChanged();
				}
			}
		}

		public GColor? UnselectedColor
		{
			get
			{
				return _unselectedColor;
			}
			set
			{
				if (_unselectedColor != value)
				{
					_unselectedColor = value;
					NotifyPropertyChanged();
				}
			}
		}

		public ItemAppearance()
		{
		}

		public event PropertyChangedEventHandler? PropertyChanged;

		void NotifyPropertyChanged([CallerMemberName] string propertyname = "")
		{
			PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyname));
		}
	}
}