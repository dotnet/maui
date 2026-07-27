using System;
using System.ComponentModel;
using System.Globalization;
using System.Runtime.CompilerServices;

namespace Maui.Controls.Sample;

public class TriggersViewModel : INotifyPropertyChanged
{
	public event PropertyChangedEventHandler PropertyChanged;

	private TriggerType _selectedTriggerType = TriggerType.PropertyTrigger;
	private bool _isToggled = false;
	private bool _isChecked = false;
	private string _dataEntryText = string.Empty;
	private string _emailEntryText = string.Empty;
	private string _phoneEntryText = string.Empty;

	public TriggerType SelectedTriggerType
	{
		get => _selectedTriggerType;
		set
		{
			if (_selectedTriggerType != value)
			{
				_selectedTriggerType = value;
				OnPropertyChanged();
				OnPropertyChanged(nameof(ShowPropertyTrigger));
				OnPropertyChanged(nameof(ShowDataTrigger));
				OnPropertyChanged(nameof(ShowEventTrigger));
				OnPropertyChanged(nameof(ShowMultiTrigger));
				OnPropertyChanged(nameof(ShowEnterExitActions));
				OnPropertyChanged(nameof(ShowStateTrigger));
				OnPropertyChanged(nameof(ShowAdaptiveTrigger));
				OnPropertyChanged(nameof(ShowCompareStateTrigger));
				OnPropertyChanged(nameof(ShowDeviceStateTrigger));
				OnPropertyChanged(nameof(ShowOrientationStateTrigger));
			}
		}
	}

	public bool IsToggled
	{
		get => _isToggled;
		set
		{
			if (_isToggled != value)
			{
				_isToggled = value;
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

	public string DataEntryText
	{
		get => _dataEntryText;
		set
		{
			if (_dataEntryText != value)
			{
				_dataEntryText = value;
				OnPropertyChanged();
				OnPropertyChanged(nameof(IsSaveButtonEnabled));
			}
		}
	}

	public string EmailEntryText
	{
		get => _emailEntryText;
		set
		{
			if (_emailEntryText != value)
			{
				_emailEntryText = value;
				OnPropertyChanged();
			}
		}
	}

	public string PhoneEntryText
	{
		get => _phoneEntryText;
		set
		{
			if (_phoneEntryText != value)
			{
				_phoneEntryText = value;
				OnPropertyChanged();
			}
		}
	}

	public bool IsSaveButtonEnabled => !string.IsNullOrEmpty(DataEntryText);
	public bool ShowPropertyTrigger => SelectedTriggerType == TriggerType.PropertyTrigger;
	public bool ShowDataTrigger => SelectedTriggerType == TriggerType.DataTrigger;
	public bool ShowEventTrigger => SelectedTriggerType == TriggerType.EventTrigger;
	public bool ShowMultiTrigger => SelectedTriggerType == TriggerType.MultiTrigger;
	public bool ShowEnterExitActions => SelectedTriggerType == TriggerType.EnterExitActions;
	public bool ShowStateTrigger => SelectedTriggerType == TriggerType.StateTrigger;
	public bool ShowAdaptiveTrigger => SelectedTriggerType == TriggerType.AdaptiveTrigger;
	public bool ShowCompareStateTrigger => SelectedTriggerType == TriggerType.CompareStateTrigger;
	public bool ShowDeviceStateTrigger => SelectedTriggerType == TriggerType.DeviceStateTrigger;
	public bool ShowOrientationStateTrigger => SelectedTriggerType == TriggerType.OrientationStateTrigger;

	public void Reset()
	{
		IsToggled = false;
		IsChecked = false;
		DataEntryText = string.Empty;
		EmailEntryText = string.Empty;
		PhoneEntryText = string.Empty;
	}

	protected void OnPropertyChanged([CallerMemberName] string propertyName = null)
	{
		PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
	}
}

public enum TriggerType
{
	PropertyTrigger,
	DataTrigger,
	EventTrigger,
	MultiTrigger,
	EnterExitActions,
	StateTrigger,
	AdaptiveTrigger,
	CompareStateTrigger,
	DeviceStateTrigger,
	OrientationStateTrigger
}

/// <summary>
/// Trigger action that performs a fade animation on visual elements
/// </summary>
public class FadeTriggerAction : TriggerAction<VisualElement>
{
	public int StartsFrom { get; set; }

	protected override void Invoke(VisualElement sender)
	{
		sender.Animate("FadeTriggerAction", new Animation((d) =>
		{
			var val = StartsFrom == 1 ? d : 1 - d;
			sender.BackgroundColor = Color.FromRgb(1, val, 1);
		}),
		length: 1000, // milliseconds
		easing: Easing.Linear);
	}
}

/// <summary>
/// Trigger action that validates numeric input and changes text color based on validity
/// </summary>
public class NumericValidationTriggerAction : TriggerAction<Entry>
{
	protected override void Invoke(Entry entry)
	{
		double result;
		bool isValid = double.TryParse(entry.Text, out result);
		entry.TextColor = isValid ? Colors.Black : Colors.Red;
		entry.BackgroundColor = isValid ? Colors.SkyBlue : Colors.Yellow;
	}
}

/// <summary>
/// Value converter that inverts a boolean value
/// </summary>
public class InverseBooleanConverter : IValueConverter
{
	public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
	{
		if (value is bool boolValue)
		{
			return !boolValue;
		}
		return false;
	}

	public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
	{
		if (value is bool boolValue)
		{
			return !boolValue;
		}
		return false;
	}
}

/// <summary>
/// Value converter that returns true if a string is not null or empty
/// </summary>
public class StringNotNullOrEmptyConverter : IValueConverter
{
	public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
	{
		return !string.IsNullOrEmpty(value as string);
	}

	/// <summary>
	/// Two-way binding is not supported. Returns <see cref="Binding.DoNothing"/> as a default.
	/// </summary>
	public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
	{
		return Binding.DoNothing;
	}
}

/// <summary>
/// Value converter that returns the length of a string
/// </summary>
public class StringLengthConverter : IValueConverter
{
	public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
	{
		if (value is string text)
		{
			return text.Length;
		}
		return 0;
	}

	/// <summary>
	/// Two-way binding is not supported. Returns <see cref="Binding.DoNothing"/> as a default.
	/// </summary>
	public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
	{
		return Binding.DoNothing;
	}
}
