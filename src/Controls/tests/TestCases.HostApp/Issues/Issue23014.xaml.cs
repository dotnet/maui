using System.Collections.ObjectModel;

namespace Maui.Controls.Sample.Issues
{
	[Issue(IssueTracker.Github, 23014, "App crashes when calling ItemsView.ScrollTo on unloaded CollectionView", PlatformAffected.All)]
	public partial class Issue23014 : ContentPage
	{
		public Issue23014ViewModel ViewModel { get; } = new();

		public Issue23014()
		{
			InitializeComponent();
			BindingContext = new Issue23014ViewModel();
		}

		private void OnButtonClicked(object sender, EventArgs e)
		{
			if (!Stack.Remove(ItemList))
			{
				Stack.Add(ItemList);
			}

			ItemList.ScrollTo(ViewModel.SelectedItem);
		}
	}

	public class Issue23014ViewModel : ViewModel
	{
		public ObservableCollection<string> Items { get; } = ["Foo", "Bar", "Baz", "Goo"];

		private string _selectedItem = string.Empty;
		public string SelectedItem
		{
			get => _selectedItem;
			set
			{
				_selectedItem = value;
				OnPropertyChanged();
			}
		}
	}

}