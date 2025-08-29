using System.ComponentModel;
namespace Maui.Controls.Sample;

public class ContentViewViewModel : INotifyPropertyChanged
{
	private string _defaultLabelText = "This is Default Page";
	public string DefaultLabelText
	{
		get => _defaultLabelText;
		set
		{
			if (_defaultLabelText != value)
			{
				_defaultLabelText = value;
				OnPropertyChanged(nameof(DefaultLabelText));
			}
		}
	}
	private bool _useControlTemplate;
	private string _contentLabel = "FirstCustomPage";
	private int _heightRequest = -1;
	private int _widthRequest = -1;
	private Color _backgroundColor = Colors.LightGray;
	private bool _isEnabled = true;
	private bool _isVisible = true;

	public bool IsVisible
	{
		get => _isVisible;
		set
		{
			if (_isVisible != value)
			{
				_isVisible = value;
				OnPropertyChanged(nameof(IsVisible));
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
				OnPropertyChanged(nameof(IsEnabled));
			}
		}
	}
	public Color BackgroundColor
	{
		get => _backgroundColor;
		set
		{
			if (_backgroundColor != value)
			{
				_backgroundColor = value;
				OnPropertyChanged(nameof(BackgroundColor));
			}
		}
	}
	public int WidthRequest
	{
		get => _widthRequest;
		set
		{
			if (_widthRequest != value)
			{
				_widthRequest = value;
				OnPropertyChanged(nameof(WidthRequest));
			}
		}
	}
	public int HeightRequest
	{
		get => _heightRequest;
		set
		{
			if (_heightRequest != value)
			{
				_heightRequest = value;
				OnPropertyChanged(nameof(HeightRequest));
			}
		}
	}
	public bool UseControlTemplate
	{
		get => _useControlTemplate;
		set
		{
			if (_useControlTemplate != value)
			{
				_useControlTemplate = value;
				OnPropertyChanged(nameof(UseControlTemplate));
			}
		}
	}
	public string ContentLabel
	{
		get => _contentLabel;
		set
		{
			if (_contentLabel != value)
			{
				_contentLabel = value;
				OnPropertyChanged(nameof(ContentLabel));
			}
		}
	}


	public event PropertyChangedEventHandler PropertyChanged;

	protected virtual void OnPropertyChanged(string propertyName)
	{
		PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
	}
}