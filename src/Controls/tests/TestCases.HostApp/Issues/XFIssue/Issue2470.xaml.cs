using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;


namespace Maui.Controls.Sample.Issues;

public class Issue2470ViewModelBase : INotifyPropertyChanged
{
	public event PropertyChangedEventHandler PropertyChanged;

	protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
	{
		PropertyChangedEventHandler handler = PropertyChanged;
		if (handler != null)
			handler(this, new PropertyChangedEventArgs(propertyName));
	}
}

public class EntryViewModel : ViewModelBase
{
	string _name;
	public string Name
	{
		get { return _name; }
		set { _name = value; OnPropertyChanged(); }
	}

	bool _selected;
	public bool Selected
	{
		get { return _selected; }
		set { _selected = value; OnPropertyChanged(); }
	}
}

public class Issue2470MainViewModel : Issue2470ViewModelBase
{
	public ObservableCollection<EntryViewModel> Entries { get; private set; }

	double _desiredCount;
	public double DesiredCount
	{
		get { return _desiredCount; }
		set
		{
			_desiredCount = value;
			OnPropertyChanged();
			GenerateEntries();
		}
	}

	bool _twoOrFive;
	public bool TwoOrFive
	{
		get { return _twoOrFive; }
		set
		{
			_twoOrFive = value;
			OnPropertyChanged();
			DesiredCount = _twoOrFive ? 5 : 2;
		}
	}

	public Issue2470MainViewModel()
	{
		Entries = new ObservableCollection<EntryViewModel>();
		TwoOrFive = false; // prime
	}

	void GenerateEntries()
	{
		Entries.Clear();
		for (var i = 0; i < DesiredCount; i++)
		{
			Entries.Add(new EntryViewModel { Name = "Entry " + i + " of " + DesiredCount });
		}
	}
}

[Issue(IssueTracker.Github, 2470, "ObservableCollection changes do not update ListView", PlatformAffected.Android)]
public partial class Issue2470 : TestShell
{
	protected override void Init()
	{
		var mainViewModel = new Issue2470MainViewModel();
		BindingContext = mainViewModel;
	}
	public Issue2470()
	{
		InitializeComponent();
	}
}
