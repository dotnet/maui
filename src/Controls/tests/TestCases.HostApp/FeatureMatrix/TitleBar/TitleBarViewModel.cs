using System.ComponentModel;
using System.Runtime.CompilerServices;
namespace Maui.Controls.Sample;

public class TitleBarViewModel : INotifyPropertyChanged
{
	private IView _titleBarContent;
	private string _title;
	private string _subtitle;
	private IView _trailingContent;
	private Color _foregroundColor = Colors.Black;
	private ImageSource _icon;

	public IView TitleBarContent
	{
		get => _titleBarContent;
		set
		{
			if (_titleBarContent != value)
			{
				_titleBarContent = value;
			}
		}
	}

	private bool _isTitleBarContentVisible = false;
	public bool IsTitleBarContentVisible
	{
		get => _isTitleBarContentVisible;
		set
		{
			if (_isTitleBarContentVisible != value)
			{
				_isTitleBarContentVisible = value;
				IsSearchBarChecked = false;
				IsHorizontalStackLayoutChecked = false;
				IsGridWithProgressBarChecked = false;

				if (!_isTitleBarContentVisible)
				{
					TitleBarContent = null;
				}
			}
		}
	}

	private bool _isSearchBarChecked = false;
	public bool IsSearchBarChecked
	{
		get => _isSearchBarChecked;
		set
		{
			if (_isSearchBarChecked != value)
			{
				_isSearchBarChecked = value;

			}
		}
	}

	private bool _isHorizontalStackLayoutChecked = false;
	public bool IsHorizontalStackLayoutChecked
	{
		get => _isHorizontalStackLayoutChecked;
		set
		{
			if (_isHorizontalStackLayoutChecked != value)
			{
				_isHorizontalStackLayoutChecked = value;
			}
		}
	}

	private bool _isGridWithProgressBarChecked = false;
	public bool IsGridWithProgressBarChecked
	{
		get => _isGridWithProgressBarChecked;
		set
		{
			if (_isGridWithProgressBarChecked != value)
			{
				_isGridWithProgressBarChecked = value;
			}
		}
	}

	public void SetSearchBarContent()
	{
		if (IsTitleBarContentVisible)
		{
			TitleBarContent = new SearchBar
			{
				Placeholder = "Search",
				PlaceholderColor = Colors.White,
				BackgroundColor = Colors.LightGray,
				MaximumWidthRequest = 300,
				HorizontalOptions = LayoutOptions.Fill,
				VerticalOptions = LayoutOptions.Center
			};
		}
	}

	public void SetHorizontalStackLayoutContent()
	{
		if (IsTitleBarContentVisible)
		{
			TitleBarContent = new HorizontalStackLayout
			{
				HorizontalOptions = LayoutOptions.Center,
				Children =
					{
						new Label { Text = "Label 1", TextColor = Colors.White, VerticalOptions = LayoutOptions.Center },
						new Label { Text = "Label 2", TextColor = Colors.White, VerticalOptions = LayoutOptions.Center }
					}
			};
		}
	}

	public void SetGridWithProgressBar()
	{
		if (IsTitleBarContentVisible)
		{
			TitleBarContent = new Grid
			{
				Children =
					{
						new ProgressBar
						{
							Margin=10,
							Progress = 0.5,
							BackgroundColor = Colors.LightGray,
							ProgressColor = Colors.White,
							HorizontalOptions = LayoutOptions.Fill,
							VerticalOptions = LayoutOptions.Center
						}
					}
			};
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

	public string Subtitle
	{
		get => _subtitle;
		set
		{
			if (_subtitle != value)
			{
				_subtitle = value;
				OnPropertyChanged();
			}
		}
	}

	public Color ForegroundColor
	{
		get => _foregroundColor;
		set
		{
			if (_foregroundColor != value)
			{
				_foregroundColor = value;
			}
		}
	}

	public ImageSource Icon
	{
		get => _icon;
		set
		{
			if (_icon != value)
			{
				_icon = value;
			}
		}
	}

	private Color _color = Color.FromArgb("#6600ff");
	public Color Color
	{
		get => _color;
		set { _color = value; }
	}

	private bool _isRedChecked = false;
	public bool IsRedChecked
	{
		get => _isRedChecked;
		set
		{
			if (_isRedChecked != value)
			{
				_isRedChecked = value;
				if (value && ShowBackgroundColor) // Check if ShowBackgroundColor is true
					Color = Colors.Red;
			}
		}
	}

	private bool _isOrangeChecked = false;
	public bool IsOrangeChecked
	{
		get => _isOrangeChecked;
		set
		{
			if (_isOrangeChecked != value)
			{
				_isOrangeChecked = value;
				if (value && ShowBackgroundColor)
					Color = Colors.Orange;
			}
		}
	}

	private bool _isVisible = true;
	public bool IsVisible
	{
		get => _isVisible;
		set
		{
			if (_isVisible != value)
			{
				_isVisible = value;
			}
		}
	}

	public FlowDirection _flowDirection = FlowDirection.LeftToRight;
	public FlowDirection FlowDirection
	{
		get => _flowDirection;
		set
		{
			if (_flowDirection != value)
			{
				_flowDirection = value;
			}
		}
	}

	private bool _showLeadingContent = false;
	public bool ShowLeadingContent
	{
		get => _showLeadingContent;
		set
		{
			if (_showLeadingContent != value)
			{
				_showLeadingContent = value;

				LeadingContent = _showLeadingContent ? new Image
				{
					Source = "dotnet_bot.png",
					HeightRequest = 60,
					Margin = 10,
					VerticalOptions = LayoutOptions.Center
				} : null;
			}
		}
	}

	private IView _leadingContent;
	public IView LeadingContent
	{
		get => _leadingContent;
		set
		{
			if (_leadingContent != value)
			{
				_leadingContent = value;
			}
		}
	}

	public IView TrailingContent
	{
		get => _trailingContent;
		set
		{
			if (_trailingContent != value)
			{
				_trailingContent = value;
			}
		}
	}
	private bool _showTrailingContent = false;
	public bool ShowTrailingContent
	{
		get => _showTrailingContent;
		set
		{
			if (_showTrailingContent != value)
			{
				_showTrailingContent = value;

				TrailingContent = _showTrailingContent ? new ImageButton
				{
					Source = "avatar.png",
					CornerRadius = 5,
					Margin = 10,
					HeightRequest = 60,
				} : null;
			}
		}
	}

	private bool _showTitle = false;
	public bool ShowTitle
	{
		get => _showTitle;
		set
		{
			if (_showTitle != value)
			{
				_showTitle = value;
				Title = _showTitle ? "My MAUI App" : string.Empty;
				OnPropertyChanged();
			}
		}
	}

	private bool _showSubtitle = false;
	public bool ShowSubtitle
	{
		get => _showSubtitle;
		set
		{
			if (_showSubtitle != value)
			{
				_showSubtitle = value;
				Subtitle = _showSubtitle ? "Demo TitleBar Integration" : string.Empty;
				OnPropertyChanged();
			}
		}
	}

	private bool _showForegroundColor = false;
	public bool ShowForegroundColor
	{
		get => _showForegroundColor;
		set
		{
			if (_showForegroundColor != value)
			{
				_showForegroundColor = value;
				ForegroundColor = _showForegroundColor ? Colors.Black : Colors.Black;
				IsWhiteForegroundChecked = false;
			}
		}
	}

	private bool _isWhiteForegroundChecked = false;
	public bool IsWhiteForegroundChecked
	{
		get => _isWhiteForegroundChecked;
		set
		{
			if (_isWhiteForegroundChecked != value)
			{
				_isWhiteForegroundChecked = value;
				if (value && ShowForegroundColor)
					ForegroundColor = Colors.White;
			}
		}
	}

	private bool _showIcon = false;
	public bool ShowIcon
	{
		get => _showIcon;
		set
		{
			if (_showIcon != value)
			{
				_showIcon = value;
				Icon = _showIcon ? ImageSource.FromFile("green.png") : null;
			}
		}
	}

	private bool _showBackgroundColor = false;
	public bool ShowBackgroundColor
	{
		get => _showBackgroundColor;
		set
		{
			if (_showBackgroundColor != value)
			{
				_showBackgroundColor = value;
				Color = _showBackgroundColor ? Color.FromArgb("#6600ff") : Color.FromArgb("#6600ff");
				IsOrangeChecked = false;
				IsRedChecked = false;
			}
		}
	}

	public event PropertyChangedEventHandler PropertyChanged;
	protected void OnPropertyChanged([CallerMemberName] string propertyName = null)
	{
		PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
	}
}