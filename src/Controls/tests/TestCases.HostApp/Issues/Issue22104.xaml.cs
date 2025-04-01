using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Windows.Input;
using Microsoft.Maui.Controls.Internals;

namespace Maui.Controls.Sample.Issues;

[XamlCompilation(XamlCompilationOptions.Compile)]
[Preserve(AllMembers = true)]
[Issue(IssueTracker.Github, 22104, "VisualState Setters not working properly on Windows for a CollectionView", PlatformAffected.UWP)]
public partial class Issue22104 : ContentPage
{
	public Issue22104()
	{
		InitializeComponent();
	}
}

internal class Issue22104ViewModel : INotifyPropertyChanged
{
	// Observable Collection for the List
	public ObservableCollection<string> List { get; set; } = new() { "One", "Two", "Three", "Four", "Five" };

	private string _selectedItem;
	public string SelectedItem
	{
		get => _selectedItem;
		set
		{
			if (_selectedItem != value)
			{
				_selectedItem = value;
				OnPropertyChanged(nameof(SelectedItem));
			}
		}
	}

	private string _labelText;
	public string LabelText
	{
		get => _labelText;
		set
		{
			if (_labelText != value)
			{
				_labelText = value;
				OnPropertyChanged(nameof(LabelText));
			}
		}
	}

	private int _selectedIndex;

	public Issue22104ViewModel()
	{
		// Initialize the SelectNextCommand
		SelectNextCommand = new RelayCommand(SelectNext);
	}

	// Command for selecting the next item
	public ICommand SelectNextCommand { get; }

	private async Task SelectNext()
	{
		await MainThread.InvokeOnMainThreadAsync(() =>
		{
			if (SelectedItem == null)
			{
				_selectedIndex = 0;
			}
			else
			{
				_selectedIndex++;
			}

			if (_selectedIndex < List.Count)
			{
				SelectedItem = List[_selectedIndex];
			}
			else
			{
				SelectedItem = null;
			}

			LabelText = SelectedItem;
		});
	}

	// INotifyPropertyChanged implementation
	public event PropertyChangedEventHandler PropertyChanged;

	protected virtual void OnPropertyChanged(string propertyName)
	{
		PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
	}
}

public class RelayCommand : ICommand
{
	private readonly Func<Task> execute;
	private readonly Func<bool> canExecute;

	public RelayCommand(Func<Task> execute, Func<bool> canExecute = null)
	{
		this.execute = execute ?? throw new ArgumentNullException(nameof(execute));
		this.canExecute = canExecute;
	}

	public event EventHandler CanExecuteChanged;

	public bool CanExecute(object parameter) => canExecute == null || canExecute();

	public async void Execute(object parameter) => await execute();

	public void RaiseCanExecuteChanged() => CanExecuteChanged?.Invoke(this, EventArgs.Empty);
}
