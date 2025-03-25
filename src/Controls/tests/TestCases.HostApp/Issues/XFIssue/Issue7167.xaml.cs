using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Windows.Input;

namespace Maui.Controls.Sample.Issues;

[Issue(IssueTracker.Github, 7167,
	"[Bug] improved observablecollection. a lot of collectionchanges. a reset is sent and listview scrolls to the top", PlatformAffected.UWP)]
public partial class Issue7167 : TestContentPage
{
	protected override void Init()
	{
		InitializeComponent();

		BindingContext = new Issue7167ViewModel();
	}

	void MyListView_OnItemSelected(object sender, SelectedItemChangedEventArgs e)
	{
		var item = e.SelectedItem;
		var index = e.SelectedItemIndex;

	}
}

internal class Issue7167ViewModel
{
	IEnumerable<string> CreateItems()
	{
		var itemCount = Items.Count();
		return Enumerable.Range(itemCount, 50).Select(num => num.ToString());
	}

	public ImprovedObservableCollection<string> Items { get; set; } = new ImprovedObservableCollection<string>();

	public ICommand AddCommand => new Command(_ => Items.Add(CreateItems().First()));
	public ICommand ClearListCommand => new Command(_ => Items.Clear());
	public ICommand AddRangeCommand => new Command(_ => Items.AddRange(CreateItems()));
	public ICommand AddRangeWithCleanCommand => new Command(_ =>
	{
		Items.Clear();
		Items.AddRange(CreateItems());
	});
}

internal class ImprovedObservableCollection<T> : ObservableCollection<T>
{
	bool _isActivated = true;
	public void AddRange(IEnumerable<T> source)
	{
		_isActivated = false;

		foreach (var item in source)
		{
			base.Items.Add(item);
		}

		_isActivated = true;

		this.OnCollectionChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Reset));
	}

	protected override void OnCollectionChanged(NotifyCollectionChangedEventArgs e)
	{
		if (_isActivated)
		{
			base.OnCollectionChanged(e);
		}
	}

}
