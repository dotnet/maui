using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows.Input;

namespace Maui.Controls.Sample.Issues;

[Issue(IssueTracker.Github, 33402, "Show ContextFlyout on long press on mobile platforms", PlatformAffected.All)]
public partial class Issue33402 : ContentPage
{
	public Issue33402()
	{
		InitializeComponent();
		BindingContext = new Issue33402ViewModel();
	}
}

public class Issue33402ViewModel : INotifyPropertyChanged
{
	string _status = "Ready";
	object _selectedItem;

	public Issue33402ViewModel()
	{
		RecordCommand = new Command<string>(action => Status = action);
		DeleteCommand = new Command<string>(item =>
		{
			Items.Remove(item);
			Status = $"deleted:{item}";
		});
	}

	public ObservableCollection<string> Items { get; } = new() { "Item1", "Item2", "Item3" };

	public ICommand RecordCommand { get; }

	public ICommand DeleteCommand { get; }

	public string CopyText => "Copy item";

	public string Status
	{
		get => _status;
		set
		{
			_status = value;
			OnPropertyChanged();
		}
	}

	public object SelectedItem
	{
		get => _selectedItem;
		set
		{
			_selectedItem = value;
			OnPropertyChanged();

			if (value is not null)
				Status = $"selected:{value}";
		}
	}

	public event PropertyChangedEventHandler PropertyChanged;

	void OnPropertyChanged([CallerMemberName] string name = null) =>
		PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
}
