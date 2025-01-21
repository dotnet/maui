
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

public class Issue27200ViewModel : INotifyPropertyChanged
{
	private bool _isRefreshing;
	private bool _showHeader;

	public event PropertyChangedEventHandler PropertyChanged;

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
	public System.Windows.Input.ICommand RefreshCommand { get; }

	public Issue27200ViewModel()
	{
		RefreshCommand = new Command(async () => await RefreshAsync());
		_ = InitItems();
	}

	public async Task RefreshAsync()
	{
		try
		{
			IsRefreshing = true;
			await InitItems();
		}
		finally
		{
			IsRefreshing = false;
		}
	}

	private async Task InitItems()
	{
		await Task.Delay(2000);

		Application.Current?.Dispatcher.Dispatch(() =>
		{
			Items.Clear();
			foreach (var vm in GetItemGroups())
			{
				Items.Add(vm);
			}
		});
	}

	private List<Issue27200ItemGroupViewModel> GetItemGroups()
	{
		return new List<Issue27200ItemGroupViewModel>
		{
			new Issue27200ItemGroupViewModel("Group 1", GetItems()),
			new Issue27200ItemGroupViewModel("Group 2", GetItems()),
		};
	}

	private List<Issue27200ItemViewModel> GetItems()
	{
		return new List<Issue27200ItemViewModel>
		{
			new() { TextLine1 = "1" },
			new() { TextLine1 = "2" },
			new() { TextLine1 = "3" },
			new() { TextLine1 = "4" },
			new() { TextLine1 = "5" },
			new() { TextLine1 = "6" },
			new() { TextLine1 = "7" },
			new() { TextLine1 = "8" },
			new() { TextLine1 = "9" },
			new() { TextLine1 = "10" },
		};
	}

	protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
	{
		PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
	}
}

public class Issue27200ItemGroupViewModel : List<Issue27200ItemViewModel>
{
	public string Name { get; private set; }

	public Issue27200ItemGroupViewModel(string name, List<Issue27200ItemViewModel> items) : base(items)
	{
		Name = name;
	}
}

public class Issue27200ItemViewModel : INotifyPropertyChanged
{
	private string _textLine1;

	public string TextLine1
	{
		get => _textLine1;
		set
		{
			if (_textLine1 != value)
			{
				_textLine1 = value;
				OnPropertyChanged();
			}
		}
	}

	public bool ShowTextLine1 => !string.IsNullOrWhiteSpace(TextLine1);


	public event PropertyChangedEventHandler PropertyChanged;

	protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
	{
		PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
	}
}
