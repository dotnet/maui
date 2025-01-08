using System.Collections.ObjectModel;

namespace Maui.Controls.Sample.Issues;

[Issue(IssueTracker.Github, 26936, "CollectionView items appear with ellipses or dots when dynamically adding more items.", PlatformAffected.macOS | PlatformAffected.iOS)]
public partial class Issue26936 : ContentPage
{
	Issue26936ViewModel _viewModel = new Issue26936ViewModel();

	public Issue26936()
	{
		InitializeComponent();
		collectionView.ItemsSource = _viewModel.Items;
		this.BindingContext = _viewModel;
	}
	private void Button_Clicked(object sender, EventArgs e)
	{
		_viewModel.Items.Add("item: " + this._viewModel.Items.Count);
	}
}

public class Issue26936ViewModel
{
	public ObservableCollection<string> Items { get; set; }
	public Issue26936ViewModel()
	{
		Items = new ObservableCollection<string>();
		Items.Add("item: " + "0");
	}
}
