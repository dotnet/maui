using System.Collections.ObjectModel;

namespace Maui.Controls.Sample.Issues;

[Issue(IssueTracker.Github, 29882, "[iOS] Crash occurs when ItemsSource is set to null in the SelectionChanged handler", PlatformAffected.iOS)]
public partial class Issue29882 : ContentPage
{
	private ObservableCollection<string> items;
	public ObservableCollection<string> Items
	{
		get => items;
		set
		{
			items = value;
			OnPropertyChanged();
		}
	}
	public Issue29882()
	{
		InitializeComponent();
		Items = new ObservableCollection<string>();
		for (int i = 1; i <= 10; i++)
		{
			Items.Add($"Item{i}");
		}
		BindingContext = this;
	}

	private void MyCollectionView_SelectionChanged(object sender, SelectionChangedEventArgs e)
	{
		Items = null;
	}
}