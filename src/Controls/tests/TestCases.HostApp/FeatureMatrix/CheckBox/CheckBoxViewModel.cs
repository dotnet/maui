using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows.Input;
using Microsoft.Maui;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Graphics;

namespace Maui.Controls.Sample;

public class CheckBoxViewModel : INotifyPropertyChanged
{
	private bool _isChecked = true;
	private Color _color = null;
	private bool _isEnabled = true;
	private bool _isVisible = true;
	private bool _hasShadow = false;
	private Shadow _shadow = null;
	private string _checkedChangedStatus = string.Empty;
	private bool _isEventStatusLabelVisible = false;
	private string _commandStatus = string.Empty;
	private bool _isCommandStatusLabelVisible = false;
	private string _commandParameter = string.Empty;

	public event PropertyChangedEventHandler PropertyChanged;

	public CheckBoxViewModel()
	{
		CheckedChangedCommand = new Command(OnCheckedChanged);
		CheckBoxCommand = new Command<string>(OnCheckBoxCommand);
		SetColorCommand = new Command<string>(OnSetColor);
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

	public Color Color
	{
		get => _color;
		set
		{
			if (_color != value)
			{
				_color = value;
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

	public string CheckedChangedStatus
	{
		get => _checkedChangedStatus;
		set
		{
			if (_checkedChangedStatus != value)
			{
				if (!string.IsNullOrEmpty(value))
				{
					IsEventStatusLabelVisible = true;
				}
				_checkedChangedStatus = value;
				OnPropertyChanged();
			}
		}
	}

	public bool IsEventStatusLabelVisible
	{
		get => _isEventStatusLabelVisible;
		set
		{
			if (_isEventStatusLabelVisible != value)
			{
				_isEventStatusLabelVisible = value;
				OnPropertyChanged();
			}
		}
	}

	public ICommand CheckedChangedCommand { get; }
	public ICommand CheckBoxCommand { get; }
	public ICommand SetColorCommand { get; }

	public string CommandParameter
	{
		get => _commandParameter;
		set
		{
			if (_commandParameter != value)
			{
				_commandParameter = value;
				OnPropertyChanged();
			}
		}
	}

	public string CommandStatus
	{
		get => _commandStatus;
		set
		{
			if (_commandStatus != value)
			{
				if (!string.IsNullOrEmpty(value))
				{
					IsCommandStatusLabelVisible = true;
				}
				_commandStatus = value;
				OnPropertyChanged();
			}
		}
	}

	public bool IsCommandStatusLabelVisible
	{
		get => _isCommandStatusLabelVisible;
		set
		{
			if (_isCommandStatusLabelVisible != value)
			{
				_isCommandStatusLabelVisible = value;
				OnPropertyChanged();
			}
		}
	}

	private void OnCheckedChanged()
	{
		CheckedChangedStatus = "CheckedChanged Triggered";
	}

	private void OnCheckBoxCommand(string parameter)
	{
		if (string.IsNullOrEmpty(parameter))
		{
			CommandStatus = "Command Executed";
		}
		else
		{
			CommandStatus = $"Command Executed: {parameter}";
		}
	}

	private void OnSetColor(string colorName)
	{
		Color = colorName switch
		{
			"Blue" => Colors.Blue,
			"Green" => Colors.Green,
			_ => null,
		};
	}

	public bool HasShadow
	{
		get => _hasShadow;
		set
		{
			if (_hasShadow != value)
			{
				_hasShadow = value;
				Shadow = value
					? new Shadow
					{
						Radius = 10,
						Opacity = 1.0f,
						Brush = Colors.Black.AsPaint(),
						Offset = new Point(5, 5)
					}
					: null;
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

	public void Reset()
	{
		IsChecked = true;
		Color = null;
		IsEnabled = true;
		IsVisible = true;
		HasShadow = false;
		CheckedChangedStatus = string.Empty;
		IsEventStatusLabelVisible = false;
		CommandStatus = string.Empty;
		IsCommandStatusLabelVisible = false;
		CommandParameter = string.Empty;
	}

	protected void OnPropertyChanged([CallerMemberName] string propertyName = null)
	{
		PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
	}
}