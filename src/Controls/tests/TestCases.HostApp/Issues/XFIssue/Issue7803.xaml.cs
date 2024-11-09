using System.Collections.ObjectModel;
using System.ComponentModel;


namespace Maui.Controls.Sample.Issues;

[Issue(IssueTracker.Github, 7803, "[Bug] CarouselView/RefreshView pull to refresh command firing twice on a single pull", PlatformAffected.All)]
public partial class Issue7803 : TestContentPage
{
	public Issue7803()
	{
		InitializeComponent();

		BindingContext = new ViewModel7803();
	}

	protected override void Init()
	{

	}
}

public class ViewModel7803 : INotifyPropertyChanged
{
	public ObservableCollection<Model7803> Items { get; set; } = new ObservableCollection<Model7803>();

	private bool _isRefreshing;

	public bool IsRefreshing
	{
		get
		{
			return _isRefreshing;
		}
		set
		{
			_isRefreshing = value;

			OnPropertyChanged("IsRefreshing");
		}
	}

	private string _text;

	public string Text
	{
		get
		{
			return _text;
		}
		set
		{
			_text = value;

			OnPropertyChanged("Text");
		}
	}

	public Command RefreshCommand { get; set; }

	public ViewModel7803()
	{
		PopulateItems();

		RefreshCommand = new Command(async () =>
		{
			IsRefreshing = true;

			await Task.Delay(2000);
			PopulateItems();

			IsRefreshing = false;
		});
	}

	void PopulateItems()
	{
		var count = Items.Count;

		for (var i = count; i < count + 10; i++)
			Items.Add(new Model7803() { Position = i });

		Text = "Count: " + Items.Count;
	}

	public event PropertyChangedEventHandler PropertyChanged;

	protected void OnPropertyChanged(string name)
	{
		PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
	}
}

public class Model7803 : INotifyPropertyChanged
{
	private int _position;

	public int Position
	{
		get
		{
			return _position;
		}
		set
		{
			_position = value;

			OnPropertyChanged("Position");
		}
	}

	public event PropertyChangedEventHandler PropertyChanged;

	protected void OnPropertyChanged(string name)
	{
		PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
	}
}
