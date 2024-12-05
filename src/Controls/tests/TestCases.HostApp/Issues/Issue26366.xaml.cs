using System.Collections.ObjectModel;

namespace Maui.Controls.Sample.Issues
{

	[Issue(IssueTracker.Github, 26366, "CollectionView ScrollOffset does not reset when the ItemSource is changed", PlatformAffected.iOS)]
	public partial class Issue26366 : ContentPage
	{
		ViewModel26366 _vm = new();

		public Issue26366()
		{
			InitializeComponent();
			BindingContext = _vm;
		}

		void CollectionView_OnScrolled(object sender, ItemsViewScrolledEventArgs e)
			=> Label.Text = e.VerticalOffset.ToString();

		void NewItemsSourceClicked(object sender, EventArgs e)
			=> _vm.Items = [.. Enumerable.Range(40, 40).Select(x => x.ToString()).ToList()];

		class ViewModel26366 : ViewModel
		{
			private ObservableCollection<string> _items = [.. Enumerable.Range(0, 40).Select(x => x.ToString()).ToList()];
			public ObservableCollection<string> Items
			{
				get => _items;
				set
				{
					_items = value;
					OnPropertyChanged();
				}
			}
		}
	}
}