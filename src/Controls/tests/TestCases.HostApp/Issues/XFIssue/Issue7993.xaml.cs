using System.Collections.ObjectModel;

namespace Maui.Controls.Sample.Issues;

[Issue(IssueTracker.Github, 7993, "[Bug] CollectionView.Scrolled event offset isn't correctly reset when items change", PlatformAffected.Android)]
public partial class Issue7993 : TestContentPage
{
	public Issue7993()
	{
		InitializeComponent();

		BindingContext = new ViewModel7993();
	}

	void CollectionView_OnScrolled(object sender, ItemsViewScrolledEventArgs e)
	{
		Label1.Text = "VerticalOffset: " + e.VerticalOffset;
	}

	void NewItemsSourceClicked(object sender, EventArgs e)
	{
		BindingContext = new ViewModel7993();
	}

	protected override void Init()
	{

	}
}

public class ViewModel7993
{
	public ObservableCollection<Model7993> Items { get; set; }

	public ViewModel7993()
	{
		var collection = new ObservableCollection<Model7993>();

		for (var i = 0; i < 20; i++)
		{
			collection.Add(new Model7993()
			{
				Text = i.ToString()
			});
		}

		Items = collection;
	}
}

public class Model7993
{
	public string Text { get; set; }

	public Model7993()
	{

	}
}