using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows.Input;
using Microsoft.Maui.Controls;

namespace Maui.Controls.Sample;

public partial class CheckBoxFeatureMatrixViewModel : INotifyPropertyChanged
{
	private bool _isChecked = true;
	private Color _color = null;
	private bool _isEnabled = true;
	private bool _isVisible = true;
	private string _checkedChangedStatus = string.Empty;
	private bool _isEventStatusLabelVisible = false;

	public event PropertyChangedEventHandler PropertyChanged;

	public CheckBoxFeatureMatrixViewModel()
	{
		CheckedChangedCommand = new Command(OnCheckedChanged);
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

	private void OnCheckedChanged()
	{
		CheckedChangedStatus = "CheckedChanged Triggered";
	}

	protected void OnPropertyChanged([CallerMemberName] string propertyName = null)
	{
		PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
	}
}