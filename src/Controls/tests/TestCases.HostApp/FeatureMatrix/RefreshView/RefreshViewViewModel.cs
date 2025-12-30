using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows.Input;

namespace Maui.Controls.Sample;

public class RefreshViewViewModel : INotifyPropertyChanged
{
	private ICommand _command;
	private object _commandParameter = "Orange";
	private bool _continue = false;
	private FlowDirection _flowDirection = FlowDirection.LeftToRight;
	private bool _isEnabled = true;
	private bool _isVisible = true;
	private bool _isRefreshing = false;
	private Color _refreshColor = Colors.Black;
	private Color _boxViewColor = Colors.Orange;
	private Shadow _shadow = null;
	private string _refreshStatusText = "None";

	public event PropertyChangedEventHandler PropertyChanged;

	public RefreshViewViewModel()
	{
		Command = new Command(async (parameter) =>
		{
			RefreshStatusText = $"Refresh Started";
			if (parameter.ToString() == "Red" || parameter.ToString() == "Green")
			{
				BoxViewColor = parameter.ToString() == "Red" ? Colors.Red : Colors.Green;
			}
			if (!Continue)
			{
				await Task.Delay(2000);
				IsRefreshing = false;
				RefreshStatusText = $"Refresh completed";
			}
		});
	}

	public ICommand Command
	{
		get => _command;
		set
		{
			if (_command != value)
			{
				_command = value;
				OnPropertyChanged();
			}
		}
	}

	public object CommandParameter
	{
		get => _commandParameter;
		set
		{
			if (_commandParameter != value)
			{
				_commandParameter = value;
				OnPropertyChanged();

				if (Command is Command cmd)
					cmd.ChangeCanExecute();
			}
		}
	}

	public bool Continue
	{
		get => _continue;
		set
		{
			if (_continue != value)
			{
				_continue = value;
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

	public bool IsRefreshing
	{
		get => _isRefreshing;
		set
		{
			if (_isRefreshing != value)
			{
				_isRefreshing = value;
				OnPropertyChanged();
			}
		}
	}

	public Color RefreshColor
	{
		get => _refreshColor;
		set
		{
			if (_refreshColor != value)
			{
				_refreshColor = value;
				OnPropertyChanged();
			}
		}
	}

	public Color BoxViewColor
	{
		get => _boxViewColor;
		set
		{
			if (_boxViewColor != value)
			{
				_boxViewColor = value;
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

	public string RefreshStatusText
	{
		get => _refreshStatusText;
		set
		{
			if (_refreshStatusText != value)
			{
				_refreshStatusText = value;
				OnPropertyChanged();
			}
		}
	}

	protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
	{
		PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
	}
}