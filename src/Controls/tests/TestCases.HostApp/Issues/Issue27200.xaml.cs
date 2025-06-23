
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
namespace Maui.Controls.Sample.Issues;

[Issue(IssueTracker.Github, 27200, "The size of the CollectionView header is incorrect when it contains a Binding on an IsVisible", PlatformAffected.iOS)]
public partial class Issue27200 : ContentPage
{
	Issue27200ViewModel _pageModel;
	public Issue27200()
	{
		InitializeComponent();
		BindingContext = _pageModel = new Issue27200ViewModel();
	}
}

class Issue27200ViewModel : INotifyPropertyChanged
{
	private bool _showHeader;

	public event PropertyChangedEventHandler PropertyChanged;

	public bool ShowHeader
	{
		get => _showHeader;
		set
		{
			if (_showHeader != value)
			{
				_showHeader = value;
				OnPropertyChanged();
			}
		}
	}

	public ObservableCollection<Issue27200ItemGroupViewModel> Items { get; } = new();

	public Issue27200ViewModel()
	{
		InitItems();
	}

	private void InitItems()
	{
		Items.Clear();
		foreach (var vm in GetItemGroups())
		{
			Items.Add(vm);
		}
	}

	private List<Issue27200ItemGroupViewModel> GetItemGroups()
	{
		return new List<Issue27200ItemGroupViewModel>
		{
			new Issue27200ItemGroupViewModel("Group 1", GetItems()),
			new Issue27200ItemGroupViewModel("Group 2", GetItems()),
		};
	}

	private List<string> GetItems()
	{
		return new List<string>
		{
			"1",
			"2",
			"3",
			"4",
			"5",
			"6",
			"7",
			"8",
			"9",
			"10"
		};
	}

	protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
	{
		PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
	}
}

class Issue27200ItemGroupViewModel : List<string>
{
	public string Name { get; private set; }

	public Issue27200ItemGroupViewModel(string name, List<string> items) : base(items)
	{
		Name = name;
	}
}
