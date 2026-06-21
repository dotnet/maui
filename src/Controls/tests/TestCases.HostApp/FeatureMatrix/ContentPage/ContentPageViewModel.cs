using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using System.Windows.Input;
using Microsoft.Maui.Controls;

namespace Maui.Controls.Sample;

public class ContentPageViewModel : INotifyPropertyChanged
{
	public event PropertyChangedEventHandler PropertyChanged;
	private View _content;
	private bool _hideSoftInputOnTapped;
	private string _title;
	private ImageSource _iconImageSource;
	private Brush _backgroundImageSource;
	private bool _isBusy;
	private Thickness _padding;
	public View Content
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
	public bool HideSoftInputOnTapped
	{
		get => _hideSoftInputOnTapped;
		set
		{
			if (_hideSoftInputOnTapped != value)
			{
				_hideSoftInputOnTapped = value;
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
	public ImageSource IconImageSource
	{
		get => _iconImageSource;
		set
		{
			if (_iconImageSource != value)
			{
				_iconImageSource = value;
				OnPropertyChanged();
			}
		}
	}
	public Brush BackgroundImageSource
	{
		get => _backgroundImageSource;
		set
		{
			if (_backgroundImageSource != value)
			{
				_backgroundImageSource = value;
				OnPropertyChanged();
			}
		}
	}

	public bool IsBusy
	{
		get => _isBusy;
		set
		{
			if (_isBusy != value)
			{
				_isBusy = value;
				OnPropertyChanged();
			}
		}
	}
	public Thickness Padding
	{
		get => _padding;
		set
		{
			if (_padding != value)
			{
				_padding = value;
				OnPropertyChanged();
			}
		}
	}
	private bool _isVisible = true;
	private FlowDirection _flowDirection = FlowDirection.LeftToRight;
	private string _keyboardTestText = string.Empty;

	/// <summary>
	/// Test text for keyboard behavior testing
	/// </summary>
	public string KeyboardTestText
	{
		get => _keyboardTestText;
		set
		{
			if (_keyboardTestText != value)
			{
				_keyboardTestText = value;
				OnPropertyChanged();
			}
		}
	}

	/// <summary>
	/// Controls whether the page is visible
	/// </summary>
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

	/// <summary>
	/// The flow direction for RTL/LTR layout
	/// </summary>
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

	// Commands for MVVM pattern
	public ICommand TogglePaddingCommand { get; }
	public ICommand SetGradientBackgroundCommand { get; }
	public ICommand ClearBackgroundCommand { get; }
	public ICommand ChangeContentCommand { get; }
	public ICommand ResetContentCommand { get; }
	public ICommand ResetAllCommand { get; }
	public ICommand ToggleVisibilityCommand { get; }
	public ICommand SetShadowCommand { get; }
	public ICommand ClearShadowCommand { get; }
	public ICommand ToggleFlowDirectionCommand { get; }

	public ContentPageViewModel()
	{
		// Initialize with default values
		Title = "Content Page Sample";
		HideSoftInputOnTapped = true;
		IsBusy = false;
		Padding = new Thickness(10);

		// Create a sample content view
		Content = CreateInitialContent();

		// Initialize commands
		TogglePaddingCommand = new Command(TogglePadding);
		SetGradientBackgroundCommand = new Command(SetGradientBackground);
		ClearBackgroundCommand = new Command(ClearBackground);
		ChangeContentCommand = new Command(ChangeContent);
		ResetContentCommand = new Command(ResetContent);
		ResetAllCommand = new Command(ResetAllProperties);
		ToggleVisibilityCommand = new Command(ToggleVisibility);
		ToggleFlowDirectionCommand = new Command(ToggleFlowDirection);
	}

	private View CreateInitialContent()
	{
		return new VerticalStackLayout
		{
			Children =
			{
				new Label
				{
					Text = "Welcome to ContentPage Feature Matrix!",
					VerticalOptions = LayoutOptions.Center,
					HorizontalOptions = LayoutOptions.Center,
					FontSize = 18,
					Margin = new Thickness(0, 20)
				},
				new Label
				{
					Text = "This demonstrates ContentPage properties with data binding.",
					VerticalOptions = LayoutOptions.Center,
					HorizontalOptions = LayoutOptions.Center,
					FontSize = 14
				}
			}
		};
	}

	private void TogglePadding()
	{
		Padding = Padding.Equals(new Thickness(10))
			? new Thickness(30, 20, 30, 20)
			: new Thickness(10);
	}

	private void SetGradientBackground()
	{
		BackgroundImageSource = new LinearGradientBrush
		{
			StartPoint = new Point(0, 0),
			EndPoint = new Point(1, 1),
			GradientStops = new GradientStopCollection
			{
				new GradientStop { Color = Colors.LightBlue, Offset = 0.0f },
				new GradientStop { Color = Colors.LightPink, Offset = 1.0f }
			}
		};
	}

	private void ClearBackground()
	{
		BackgroundImageSource = null;
	}

	private void ChangeContent()
	{
		Content = new VerticalStackLayout
		{
			Children =
			{
				new Label
				{
					Text = "Content has been dynamically changed!",
					VerticalOptions = LayoutOptions.Center,
					HorizontalOptions = LayoutOptions.Center,
					FontSize = 16,
					FontAttributes = FontAttributes.Bold,
					TextColor = Colors.Red,
					Margin = new Thickness(0, 20)
				},
				new Button
				{
					Text = "Reset Content",
					Command = ResetContentCommand
				}
			}
		};
	}

	private void ResetContent()
	{
		Content = CreateInitialContent();
	}

	private void ToggleVisibility()
	{
		IsVisible = !IsVisible;
	}
	private void ToggleFlowDirection()
	{
		FlowDirection = FlowDirection == FlowDirection.LeftToRight
			? FlowDirection.RightToLeft
			: FlowDirection.LeftToRight;
	}

	public void ResetAllProperties()
	{
		// Reset to initial values
		Title = "Content Page Sample";
		HideSoftInputOnTapped = true;
		IsBusy = false;
		Padding = new Thickness(10);
		IconImageSource = null;
		BackgroundImageSource = Colors.White;
		Content = CreateInitialContent();
		IsVisible = true;
		FlowDirection = FlowDirection.LeftToRight;
		KeyboardTestText = string.Empty;
	}

	protected void OnPropertyChanged([CallerMemberName] string propertyName = null)
	{
		PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
	}
}