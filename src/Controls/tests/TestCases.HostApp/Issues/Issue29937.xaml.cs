using System.Collections.ObjectModel;

namespace Maui.Controls.Sample.Issues;

[Issue(IssueTracker.Github, 29937, "[iOS/MacCatalyst] Setting SelectedItem Programmatically and Then Immediately Setting ItemsSource to Null Causes a Crash", PlatformAffected.iOS)]
public partial class Issue29937 : ContentPage
{
	private ObservableCollection<string> _items;

	public ObservableCollection<string> Items
	{
		get => _items;
		set
		{
			_items = value;
			OnPropertyChanged(nameof(Items));
		}
	}
	public Issue29937()
	{
		InitializeComponent();
		Items = new ObservableCollection<string>
			{
				"Item 1",
				"Item 2",
				"Item 3",
				"Item 4",
				"Item 5"
			};
		BindingContext = this;
	}

	void Button_Clicked(object sender, EventArgs e)
	{
		cView.SelectedItem = "Item 1";
		cView.ItemsSource = null;
	}
}