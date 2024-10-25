namespace Maui.Controls.Sample.Issues;

[Issue(IssueTracker.Github, 3798, "[Android] SeparatorColor of ListView is NOT updated dynamically")]
public partial class Issue3798 : TestContentPage
{
	public Issue3798()
	{
		InitializeComponent();
		this.BindingContext = new[] { "item1", "item2", "item3" };
	}

	bool _showRedSeparator;

	void OnItemTapped(object sender, ItemTappedEventArgs e)
	{

		if (e == null)
			return; // has been set to null, do not 'process' tapped event

		_showRedSeparator = !_showRedSeparator;
		//Uncomment the below code to test ListView SeparatorVisibility (Updating dynamically)
		//listView.SeparatorVisibility = _showRedSeparator ? SeparatorVisibility.None : SeparatorVisibility.Default;

		listView.SeparatorColor = _showRedSeparator ? Colors.Red : Colors.Green;

		((ListView)sender).SelectedItem = null; // de-select the row

	}

	protected override void Init()
	{

	}
}