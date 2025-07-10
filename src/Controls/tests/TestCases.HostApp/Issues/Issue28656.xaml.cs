using System.Collections.ObjectModel;

namespace Maui.Controls.Sample.Issues;

[Issue(IssueTracker.Github, 28656, "CollectionView CollectionViewHandler2 does not change ItemsLayout on DataTrigger", PlatformAffected.iOS)]
public partial class Issue28656 : ContentPage
{
	public Issue28656()
	{
		InitializeComponent();
		BindingContext = new Issue28656ViewModel();
	}

	public partial class Issue28656ViewModel : ViewModel
	{
		private ItemsLayout _itemsLayout = new GridItemsLayout(2, ItemsLayoutOrientation.Vertical);
		public ItemsLayout ItemsLayout
		{
			get => _itemsLayout;
			set
			{
				_itemsLayout = value;
				OnPropertyChanged();
			}
		}

		private bool _isGridLayout = true;
		public bool IsGridLayout
		{
			get => _isGridLayout;
			set
			{
				_isGridLayout = value;
				OnPropertyChanged();
			}
		}

		public ObservableCollection<string> Items => [.. Enumerable.Range(0, 100).Select(i => $"Item {i}")];

		public Command ChangeLayoutCommand { get; }

		public Issue28656ViewModel()
		{
			ChangeLayoutCommand = new Command(() =>
			{
				IsGridLayout = !IsGridLayout;
				ItemsLayout = IsGridLayout ? new GridItemsLayout(2, ItemsLayoutOrientation.Vertical) : new LinearItemsLayout(ItemsLayoutOrientation.Vertical);
			});
		}
	}
}