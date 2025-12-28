using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace Maui.Controls.Sample.Issues;

[Issue(IssueTracker.Github, 33264, "RadioButtonGroup not working with CollectionView", PlatformAffected.All)]
public partial class Issue33264 : ContentPage
{
	public Issue33264()
	{
		InitializeComponent();
		BindingContext = new Issue33264ViewModel();
	}

	protected override void OnAppearing()
	{
		base.OnAppearing();
		Dispatcher.DispatchDelayed(TimeSpan.FromMilliseconds(500), () =>
		{
			CaptureState("OnAppearing");
		});
	}

	private void CaptureState(string context)
	{
		var vm = BindingContext as Issue33264ViewModel;
		Console.WriteLine($"=== STATE CAPTURE: {context} ===");
		Console.WriteLine($"SelectedValue: {vm?.SelectedValue ?? "null"}");
		Console.WriteLine("=== END STATE CAPTURE ===");
	}
}

public class Issue33264ViewModel : INotifyPropertyChanged
{
	private string _selectedValue;

	public ObservableCollection<string> Choices { get; } = new ObservableCollection<string>
	{
		"Choice 1",
		"Choice 2",
		"Choice 3"
	};

	public string SelectedValue
	{
		get => _selectedValue;
		set
		{
			if (_selectedValue != value)
			{
				Console.WriteLine($"=== BINDING UPDATE: SelectedValue changing from '{_selectedValue}' to '{value}' ===");
				_selectedValue = value;
				OnPropertyChanged();
				OnPropertyChanged(nameof(SelectedValueDisplay));
			}
		}
	}

	public string SelectedValueDisplay => string.IsNullOrEmpty(SelectedValue) ? "None" : SelectedValue;

	public event PropertyChangedEventHandler PropertyChanged;

	protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
	{
		PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
	}
}
