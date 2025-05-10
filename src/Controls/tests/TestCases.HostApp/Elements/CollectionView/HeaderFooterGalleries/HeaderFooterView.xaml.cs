using System.Windows.Input;

namespace Maui.Controls.Sample.CollectionViewGalleries.HeaderFooterGalleries
{
	public partial class HeaderFooterView : ContentPage
	{
		readonly HeaderFooterViewModel _viewModel = new HeaderFooterViewModel(0);

		public HeaderFooterView()
		{
			InitializeComponent();

			CollectionView.ItemTemplate = ExampleTemplates.PhotoTemplate();
			CollectionView.BindingContext = _viewModel;
		}
	}

	internal class HeaderFooterViewModel : DemoFilteredItemSource
	{
		public HeaderFooterViewModel(int count = 50, Func<string, CollectionViewGalleryTestItem, bool> filter = null) : base(count, filter)
		{
		}

		public ICommand AddCommand => new Command(AddItemsAsync);

		public ICommand ClearCommand => new Command(() => Items.Clear());

		public string HeaderText => "This Is A Header";

		public string FooterText => "This Is A Footer";

		void AddItemsAsync()
		{
			AddItems(Items, 2);
		}
	}
}