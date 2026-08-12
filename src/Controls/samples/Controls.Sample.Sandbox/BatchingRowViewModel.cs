using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace Maui.Controls.Sample;

public sealed class BatchingRowViewModel : INotifyPropertyChanged
{
	readonly int _index;
	int _generation;
	string _labelText = string.Empty;
	string _buttonText = string.Empty;
	string _entryText = string.Empty;
	string _placeholder = string.Empty;
	Color _accentColor = Colors.Blue;
	Color _mutedColor = Colors.LightGray;
	double _opacity = 1;
	bool _isOn;
	double _sliderValue;
	double _progress;

	public BatchingRowViewModel(int index)
	{
		_index = index;
		Reset();
	}

	public event PropertyChangedEventHandler? PropertyChanged;

	public string EntryAutomationId => $"Entry-{_index}";

	public string SwitchAutomationId => $"Switch-{_index}";

	public string LabelText
	{
		get => _labelText;
		private set => SetProperty(ref _labelText, value);
	}

	public string ButtonText
	{
		get => _buttonText;
		private set => SetProperty(ref _buttonText, value);
	}

	public string EntryText
	{
		get => _entryText;
		private set => SetProperty(ref _entryText, value);
	}

	public string Placeholder
	{
		get => _placeholder;
		private set => SetProperty(ref _placeholder, value);
	}

	public Color AccentColor
	{
		get => _accentColor;
		private set => SetProperty(ref _accentColor, value);
	}

	public Color MutedColor
	{
		get => _mutedColor;
		private set => SetProperty(ref _mutedColor, value);
	}

	public double Opacity
	{
		get => _opacity;
		private set => SetProperty(ref _opacity, value);
	}

	public bool IsOn
	{
		get => _isOn;
		private set => SetProperty(ref _isOn, value);
	}

	public double SliderValue
	{
		get => _sliderValue;
		private set => SetProperty(ref _sliderValue, value);
	}

	public double Progress
	{
		get => _progress;
		private set => SetProperty(ref _progress, value);
	}

	public void Advance()
	{
		_generation++;
		var phase = (_generation + _index) % 10;
		var alternate = (_generation + _index) % 2 == 0;

		LabelText = $"Label {_index}: generation {_generation}";
		ButtonText = $"Button {_index}: {_generation}";
		EntryText = $"Entry {_index}: {_generation}";
		Placeholder = $"Placeholder {_generation}";
		AccentColor = alternate ? Colors.DeepPink : Colors.DodgerBlue;
		MutedColor = alternate ? Colors.Gold : Colors.LightGray;
		Opacity = 0.55 + phase * 0.045;
		IsOn = alternate;
		SliderValue = phase / 10d;
		Progress = ((phase + 3) % 10) / 10d;
	}

	public void AdvanceLabelOnly()
	{
		_generation++;
		LabelText = $"Label {_index}: generation {_generation}";
	}

	public void Reset()
	{
		_generation = 0;
		LabelText = $"Label {_index}: generation 0";
		ButtonText = $"Button {_index}: 0";
		EntryText = $"Entry {_index}: 0";
		Placeholder = "Type here";
		AccentColor = Colors.DodgerBlue;
		MutedColor = Colors.LightGray;
		Opacity = 1;
		IsOn = false;
		SliderValue = 0.25;
		Progress = 0.1;
	}

	void SetProperty<T>(ref T storage, T value, [CallerMemberName] string? propertyName = null)
	{
		if (EqualityComparer<T>.Default.Equals(storage, value))
			return;

		storage = value;
		PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
	}
}
